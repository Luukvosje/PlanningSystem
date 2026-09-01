using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Planning.Api.Authorization;
using Planning.Api.Swagger;
using Planning.Api.Middleware;
using Planning.Api.Services;
using Planning.Application;
using Planning.Application.Common;
using Planning.Domain.Enums;
using Planning.Domain.Modules;
using Planning.Infrastructure;
using Planning.Infrastructure.Data;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.RateLimiting;
using Microsoft.Extensions.FileProviders;

var builder = WebApplication.CreateBuilder(args);

if (builder.Environment.IsEnvironment("Test"))
{
    builder.Configuration.AddUserSecrets<Program>(optional: true);
}

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration, builder.Environment.ContentRootPath);

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserContext, CurrentUserContext>();
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();

// Origins come from configuration (Cors:AllowedOrigins, or Cors__AllowedOrigins__0 as an
// environment variable in production) so that deploying to a new domain never needs a code change.
// An empty list is fatal rather than permissive: a silently origin-less policy would look like a
// broken frontend in production and invite someone to "fix" it with AllowAnyOrigin.
var allowedOrigins = builder.Configuration
    .GetSection("Cors:AllowedOrigins")
    .Get<string[]>() ?? [];

if (allowedOrigins.Length == 0)
{
    throw new InvalidOperationException(
        "No CORS origins configured. Set Cors:AllowedOrigins (e.g. Cors__AllowedOrigins__0=https://app.example.com).");
}

builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
    {
        policy.WithOrigins(allowedOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

// Behind the reverse proxy the app only ever sees the proxy's IP and a plain http scheme.
// Without this, generated links and any scheme-dependent behaviour are wrong. KnownNetworks and
// KnownProxies are cleared because the proxy sits on a container network with an address we do
// not know up front; only the proxy can reach the container, so nothing else can spoof these.
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});

builder.Services.AddHealthChecks()
    .AddDbContextCheck<ApplicationDbContext>("database");

// The anonymous auth endpoints are the only ones an attacker can hammer for free. Partitioning by
// client IP works because UseForwardedHeaders (below) resolves the real address rather than the
// proxy's. Both limits are generous for a person and useless for a script.
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    // Sending mail costs money and lands in someone else's inbox, so this is the tighter of the two.
    options.AddPolicy("PasswordReset", context => RateLimitPartition.GetFixedWindowLimiter(
        context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
        _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = 5,
            Window = TimeSpan.FromMinutes(15),
        }));

    // Guards against credential stuffing on login and account enumeration on register.
    options.AddPolicy("Auth", context => RateLimitPartition.GetFixedWindowLimiter(
        context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
        _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = 20,
            Window = TimeSpan.FromMinutes(5),
        }));
});

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    // Without this, Swashbuckle ignores the C# nullable annotations: every property comes out
    // optional and nullable, even a non-nullable `string Title`. The generated TypeScript client
    // then types everything as `string | null | undefined`, which is what the hand-written
    // normalizers in the frontend existed to paper over.
    options.SupportNonNullableReferenceTypes();
    options.SchemaFilter<NonNullableAsRequiredSchemaFilter>();

    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Planning API",
        Version = "v1",
        Description = "Multi-tenant Planning SaaS foundation"
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter a valid JWT token."
    });

    options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference("Bearer", document)] = []
    });
});

var jwtSettings = builder.Configuration.GetSection("Jwt");
var signingKey = jwtSettings["Key"]
    ?? throw new InvalidOperationException("JWT signing key is not configured.");

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings["Issuer"],
            ValidAudience = jwtSettings["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKey)),
            RoleClaimType = System.Security.Claims.ClaimTypes.Role
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireOwnerOrAdmin", policy =>
        policy.RequireRole("Owner", "Admin"));

    options.AddPolicy("CanManagePlanning", policy =>
        policy.RequireRole("Owner", "Admin", "Planner"));

    options.AddPolicy("RequirePlanningModule", policy =>
        policy.Requirements.Add(new ModuleRequirement(AppModule.Planning)));

    options.AddPolicy("RequireKlantModule", policy =>
        policy.Requirements.Add(new ModuleRequirement(AppModule.Klant)));

    options.AddPolicy("RequireBeheerModule", policy =>
        policy.Requirements.Add(new ModuleRequirement(AppModule.Beheer)));
});

builder.Services.AddScoped<IAuthorizationHandler, ModuleAuthorizationHandler>();

var app = builder.Build();

if (app.Environment.IsEnvironment("Test"))
{
    using var scope = app.Services.CreateScope();
    await scope.ServiceProvider.GetRequiredService<ITestDataSeeder>().InitializeAsync();
}
else
{
    // A deploy ships code and schema together, so the container migrates itself on boot rather
    // than relying on a remembered manual step. Safe because exactly one API instance runs; with
    // a second instance this races and must move to a one-shot job before the app starts.
    using var scope = app.Services.CreateScope();
    var database = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>().Database;
    var pending = (await database.GetPendingMigrationsAsync()).ToArray();

    if (pending.Length > 0)
    {
        app.Logger.LogInformation("Applying {Count} pending migration(s): {Migrations}",
            pending.Length, string.Join(", ", pending));
        await database.MigrateAsync();
    }
}

app.UseForwardedHeaders();

app.UseMiddleware<GlobalExceptionMiddleware>();

if (app.Environment.IsDevelopment() || app.Environment.IsEnvironment("Test"))
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Planning API v1");
    });
}

// No UseHttpsRedirection here: in production the container listens on plain HTTP and is only
// reachable over the internal network. The reverse proxy terminates TLS and does the
// http->https redirect. Redirecting again inside the container would loop.

// Drives FluentValidation's built-in default messages (e.g. "'{Field}' must not be empty.")
// into the requester's language. The frontend sends this as a plain Accept-Language
// header matching its active i18n locale ('nl' or 'en'); custom .WithMessage(...) text and
// Application/Domain-layer messages are NOT covered by this (they're static English strings)
// — those are translated client-side, see Planning.Web/app/utils/backendMessages.ts.
app.UseRequestLocalization(new RequestLocalizationOptions()
    .SetDefaultCulture("nl")
    .AddSupportedCultures("nl", "en")
    .AddSupportedUICultures("nl", "en"));

app.UseRateLimiter();
app.UseCors("Frontend");

var imgPath = Path.Combine(app.Environment.ContentRootPath, "img");
Directory.CreateDirectory(Path.Combine(imgPath, "logos"));
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(imgPath),
    RequestPath = "/img",
});

app.UseAuthentication();
app.UseMiddleware<OrganizationContextMiddleware>();
app.UseAuthorization();
app.MapControllers();

// Split deliberately: /health/live answers "is this process up" and is what the container
// healthcheck and the proxy poll, so a database blip must not restart a healthy API.
// /health/ready adds the database and is what a deploy checks before declaring itself done.
app.MapHealthChecks("/health/live", new HealthCheckOptions { Predicate = _ => false });
app.MapHealthChecks("/health/ready");

app.Run();
