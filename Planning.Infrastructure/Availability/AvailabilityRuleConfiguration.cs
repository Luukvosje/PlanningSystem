using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Planning.Domain.Availability;
using Planning.Domain.Organizations;
using Planning.Domain.Users;

namespace Planning.Infrastructure.Availability;

public class AvailabilityRuleConfiguration : IEntityTypeConfiguration<AvailabilityRule>
{
    public void Configure(EntityTypeBuilder<AvailabilityRule> builder)
    {
        builder.ToTable("AvailabilityRules");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.OrganizationId)
            .IsRequired();

        builder.Property(x => x.EmployeeId)
            .IsRequired();

        builder.Property(x => x.Type)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(x => x.Weekday)
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(x => x.Date);

        builder.Property(x => x.StartTime)
            .IsRequired();

        builder.Property(x => x.EndTime)
            .IsRequired();

        builder.Property(x => x.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(x => x.Reason)
            .HasMaxLength(500);

        builder.Property(x => x.ApprovalStatus)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(x => x.DecidedByUserId)
            .IsRequired(false);

        builder.Property(x => x.DecidedAtUtc)
            .IsRequired(false);

        builder.Property(x => x.CreatedAtUtc)
            .IsRequired();

        builder.Property(x => x.UpdatedAtUtc)
            .IsRequired();

        builder.HasIndex(x => x.OrganizationId);
        builder.HasIndex(x => x.EmployeeId);
        builder.HasIndex(x => new { x.OrganizationId, x.EmployeeId, x.Type });
        builder.HasIndex(x => new { x.OrganizationId, x.EmployeeId, x.Date })
            .HasFilter($"[{nameof(AvailabilityRule.Type)}] = 'OneTime'");

        // The request inbox reads by organization and approval state.
        builder.HasIndex(x => new { x.OrganizationId, x.ApprovalStatus });

        builder.HasOne<Organization>()
            .WithMany()
            .HasForeignKey(x => x.OrganizationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(x => x.EmployeeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(x => x.DecidedByUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
