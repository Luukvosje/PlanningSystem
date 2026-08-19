using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Localization;
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

builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
    {
        policy.WithOrigins(
                "http://localhost:3000",
                "http://127.0.0.1:3000")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
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

app.UseMiddleware<GlobalExceptionMiddleware>();

if (app.Environment.IsDevelopment() || app.Environment.IsEnvironment("Test"))
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Planning API v1");
    });
}
else
{
    app.UseHttpsRedirection();
}

// Drives FluentValidation's built-in default messages (e.g. "'{Field}' must not be empty.")
// into the requester's language. The frontend sends this as a plain Accept-Language
// header matching its active i18n locale ('nl' or 'en'); custom .WithMessage(...) text and
// Application/Domain-layer messages are NOT covered by this (they're static English strings)
// — those are translated client-side, see Planning.Web/app/utils/backendMessages.ts.
app.UseRequestLocalization(new RequestLocalizationOptions()
    .SetDefaultCulture("nl")
    .AddSupportedCultures("nl", "en")
    .AddSupportedUICultures("nl", "en"));

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

app.Run();
