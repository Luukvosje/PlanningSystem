using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Planning.Domain.Availability;
using Planning.Domain.Enums;
using Planning.Domain.Organizations;
using Planning.Domain.Users;

namespace Planning.Infrastructure.Availability;

public class EmployeeAvailabilityConfiguration : IEntityTypeConfiguration<EmployeeAvailability>
{
    public void Configure(EntityTypeBuilder<EmployeeAvailability> builder)
    {
        builder.ToTable("EmployeeAvailabilities");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.OrganizationId)
            .IsRequired();

        builder.Property(x => x.UserId)
            .IsRequired();

        builder.Property(x => x.Date)
            .IsRequired();

        builder.Property(x => x.Type)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(x => x.DayPart)
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(x => x.StartTime);

        builder.Property(x => x.EndTime);

        builder.Property(x => x.IsAvailable)
            .IsRequired();

        builder.Property(x => x.Source)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(x => x.LastModifiedByUserId)
            .IsRequired();

        builder.Property(x => x.Note)
            .HasMaxLength(1000);

        builder.Property(x => x.CreatedAtUtc)
            .IsRequired();

        builder.Property(x => x.UpdatedAtUtc)
            .IsRequired();

        builder.HasIndex(x => x.OrganizationId);
        builder.HasIndex(x => x.UserId);
        builder.HasIndex(x => new { x.OrganizationId, x.UserId, x.Date });
        builder.HasIndex(x => new { x.OrganizationId, x.UserId, x.Date, x.Type, x.DayPart })
            .IsUnique()
            .HasFilter($"[{nameof(EmployeeAvailability.Type)}] = '{AvailabilityType.DayPart}'");

        builder.HasOne<Organization>()
            .WithMany()
            .HasForeignKey(x => x.OrganizationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(x => x.LastModifiedByUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
