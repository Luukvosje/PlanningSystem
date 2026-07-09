using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Planning.Domain.Customers;
using Planning.Domain.Organizations;
using Planning.Domain.Planning;
using Planning.Domain.Users;

namespace Planning.Infrastructure.Planning;

public class PlanningRecordConfiguration : IEntityTypeConfiguration<PlanningRecord>
{
    public void Configure(EntityTypeBuilder<PlanningRecord> builder)
    {
        builder.ToTable("PlanningRecords");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.OrganizationId)
            .IsRequired();

        builder.Property(x => x.CustomerId)
            .IsRequired(false);

        builder.Property(x => x.AssignedUserId)
            .IsRequired();

        builder.Property(x => x.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.Description)
            .HasMaxLength(4000);

        builder.Property(x => x.Notes)
            .HasMaxLength(4000);

        builder.Property(x => x.Color)
            .IsRequired()
            .HasMaxLength(7)
            .HasDefaultValue(PlanningRecord.DefaultColor);

        builder.Property(x => x.StartUtc)
            .IsRequired();

        builder.Property(x => x.EndUtc)
            .IsRequired();

        builder.Property(x => x.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(x => x.CreatedAtUtc)
            .IsRequired();

        builder.Property(x => x.UpdatedAtUtc)
            .IsRequired();

        builder.HasIndex(x => x.OrganizationId);
        builder.HasIndex(x => x.CustomerId);
        builder.HasIndex(x => x.AssignedUserId);
        builder.HasIndex(x => x.StartUtc);
        builder.HasIndex(x => new { x.OrganizationId, x.StartUtc, x.EndUtc });

        builder.HasOne<Organization>()
            .WithMany()
            .HasForeignKey(x => x.OrganizationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<Customer>()
            .WithMany()
            .HasForeignKey(x => x.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(x => x.AssignedUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
