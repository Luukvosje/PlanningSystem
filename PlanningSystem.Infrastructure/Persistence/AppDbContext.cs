using Microsoft.EntityFrameworkCore;
using PlanningSystem.Domain.Entities;

namespace PlanningSystem.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Organization> Organizations => Set<Organization>();
    public DbSet<OrganizationUserMap> OrganizationUserMaps => Set<OrganizationUserMap>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Shift> Shifts => Set<Shift>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Shift>()
            .HasOne(s => s.Organization)
            .WithMany(o => o.Shifts)
            .HasForeignKey(s => s.OrganizationId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Shift>()
            .HasOne(s => s.Worker)
            .WithMany()
            .HasForeignKey(s => s.WorkerId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<OrganizationUserMap>()
            .HasKey(ou => ou.Id);

        modelBuilder.Entity<OrganizationUserMap>()
            .HasOne(ou => ou.Organization)
            .WithMany(o => o.OrganizationUserMaps)
            .HasForeignKey(ou => ou.OrganizationId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<OrganizationUserMap>()
            .Property(ou => ou.Role)
            .HasConversion<string>()
            .IsRequired();
    }
}
