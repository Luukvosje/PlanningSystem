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
            options.UseSqlServer(connectionString, sqlOptions =>
                sqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 5,
                    maxRetryDelay: TimeSpan.FromSeconds(30),
                    errorNumbersToAdd: null)));

        services.AddSingleton<IOrganizationLogoStorage>(
            _ => new OrganizationLogoStorage(Path.Combine(contentRootPath, "img", "logos")));

        services.AddScoped<IPasswordHasher, BcryptPasswordHasher>();

        services.AddScoped<IOrganizationRepository, OrganizationRepository>();
        services.AddScoped<IAccountRepository, AccountRepository>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
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
