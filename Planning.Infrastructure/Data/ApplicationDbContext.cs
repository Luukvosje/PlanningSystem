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
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<User> Users => Set<User>();
    public DbSet<OrganizationInvite> OrganizationInvites => Set<OrganizationInvite>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<PlanningRecord> PlanningRecords => Set<PlanningRecord>();
    public DbSet<AvailabilityRule> AvailabilityRules => Set<AvailabilityRule>();
    public DbSet<OrganizationModule> OrganizationModules => Set<OrganizationModule>();
    public DbSet<UserModule> UserModules => Set<UserModule>();

    /// <summary>
    /// Every DateTime column in this schema holds UTC (they are all named <c>*Utc</c>). Npgsql
    /// refuses to write a non-UTC kind to <c>timestamptz</c>, and on read a kindless value would be
    /// serialized without a <c>Z</c> and read as local time by every client.
    /// See <see cref="UtcDateTimeConverter"/>.
    /// </summary>
    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.Properties<DateTime>().HaveConversion<UtcDateTimeConverter>();
        configurationBuilder.Properties<DateTime?>().HaveConversion<NullableUtcDateTimeConverter>();

        base.ConfigureConventions(configurationBuilder);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
