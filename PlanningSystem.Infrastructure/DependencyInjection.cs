using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PlanningSystem.Application.Interfaces;
using PlanningSystem.Domain.Services;
using PlanningSystem.Infrastructure.Persistence;
using PlanningSystem.Infrastructure.Repositories;

namespace PlanningSystem.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(connectionString, opt => opt.CommandTimeout(600)));

        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IOrganizationRepository, OrganizationRepository>();
        services.AddScoped<IShiftRepository, ShiftRepository>();

        services.AddScoped<ShiftSchedulingService>();
        services.AddScoped<OrganizationMembershipService>();
        services.AddScoped<OrganizationAuthorizationService>();

        return services;
    }
}
