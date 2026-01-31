using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using PlanningSystem.Interfaces.DAL;
using PlanningSystem.Models.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime;
using System.Text;
using System.Threading.Tasks;

namespace PlanningSystem.DAL
{
    public class AppDbContext : DbContext
    {
        private readonly IAppSettings? _settings;
        public AppDbContext(DbContextOptions<AppDbContext> options, IAppSettings? settings = null)
           : base(options)
        {
            _settings = settings;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured && _settings != null)
            {
                optionsBuilder.UseSqlServer(_settings.ConnectionString);
            }

        }
        public DbSet<Organization> Organizations { get; set; }
        public DbSet<OrganizationUserMap> OrganizationUserMaps { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Shift> Shifts { get; set; }

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
}
