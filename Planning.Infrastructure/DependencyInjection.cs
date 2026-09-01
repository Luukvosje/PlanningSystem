using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Planning.Application.Common;
using Planning.Domain.Auth;
using Planning.Domain.Availability;
using Planning.Domain.Customers;
using Planning.Domain.Invites;
using Planning.Domain.Modules;
using Planning.Domain.Organizations;
using Planning.Domain.Planning;
using Planning.Domain.Users;
using Planning.Infrastructure.Auth;
using Planning.Infrastructure.Availability;
using Planning.Infrastructure.Customers;
using Planning.Infrastructure.Data;
using Planning.Infrastructure.Email;
using Planning.Infrastructure.Invites;
using Planning.Infrastructure.Modules;
using Planning.Infrastructure.Organizations;
using Planning.Infrastructure.Planning;
using Planning.Infrastructure.Security;
using Planning.Infrastructure.Storage;
using Planning.Infrastructure.Users;

namespace Planning.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration,
        string contentRootPath)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(connectionString, npgsqlOptions =>
                npgsqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 5,
                    maxRetryDelay: TimeSpan.FromSeconds(30),
                    errorCodesToAdd: null)));

        services.AddSingleton<IOrganizationLogoStorage>(
            _ => new OrganizationLogoStorage(Path.Combine(contentRootPath, "img", "logos")));

        services.Configure<EmailOptions>(configuration.GetSection(EmailOptions.SectionName));

        // Without SMTP settings the app logs what it would have sent instead of failing. That
        // keeps local development free of mail credentials and makes it impossible to mail a real
        // customer by accident, while the reset link stays visible in the console for testing.
        var emailOptions = configuration.GetSection(EmailOptions.SectionName).Get<EmailOptions>();

        if (emailOptions?.IsConfigured == true)
        {
            services.AddScoped<IEmailSender, SmtpEmailSender>();
        }
        else
        {
            services.AddScoped<IEmailSender, LogOnlyEmailSender>();
        }

        services.AddScoped<IPasswordHasher, BcryptPasswordHasher>();

        services.AddScoped<IOrganizationRepository, OrganizationRepository>();
        services.AddScoped<IAccountRepository, AccountRepository>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
        services.AddScoped<IPasswordResetTokenRepository, PasswordResetTokenRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IOrganizationInviteRepository, OrganizationInviteRepository>();
        services.AddScoped<ICustomerRepository, CustomerRepository>();
        services.AddScoped<IPlanningRecordRepository, PlanningRecordRepository>();
        services.AddScoped<IAvailabilityRuleRepository, AvailabilityRuleRepository>();
        services.AddScoped<IModuleRepository, ModuleRepository>();
        services.AddScoped<ITestDataSeeder, TestDataSeeder>();

        return services;
    }
}
