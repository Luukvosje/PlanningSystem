using Microsoft.EntityFrameworkCore;
using Planning.Domain.Auth;
using Planning.Domain.Availability;
using Planning.Domain.Customers;
using Planning.Domain.Invites;
using Planning.Domain.Modules;
using Planning.Domain.Organizations;
using Planning.Domain.Planning;
using Planning.Domain.Users;

namespace Planning.Infrastructure.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Organization> Organizations => Set<Organization>();
    public DbSet<Account> Accounts => Set<Account>();
    public DbSet<User> Users => Set<User>();
    public DbSet<OrganizationInvite> OrganizationInvites => Set<OrganizationInvite>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<PlanningRecord> PlanningRecords => Set<PlanningRecord>();
    public DbSet<AvailabilityRule> AvailabilityRules => Set<AvailabilityRule>();
    public DbSet<OrganizationModule> OrganizationModules => Set<OrganizationModule>();
    public DbSet<UserModule> UserModules => Set<UserModule>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
