using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Planning.Application.Auth;
using Planning.Application.Availability;
using Planning.Application.Customers;
using Planning.Application.Invites;
using Planning.Application.Modules;
using Planning.Application.Organizations;
using Planning.Application.Planning;
using Planning.Application.Users;

namespace Planning.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssemblyContaining<ApplicationAssemblyMarker>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IInviteService, InviteService>();
        services.AddScoped<IOrganizationService, OrganizationService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<ICustomerService, CustomerService>();
        services.AddScoped<IPlanningService, PlanningService>();
        services.AddScoped<IAvailabilityService, AvailabilityService>();
        services.AddScoped<IModuleService, ModuleService>();

        return services;
    }
}
