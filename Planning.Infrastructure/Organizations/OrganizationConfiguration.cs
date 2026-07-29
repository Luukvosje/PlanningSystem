using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Planning.Domain.Organizations;
using Planning.Infrastructure.Data;

namespace Planning.Infrastructure.Organizations;

public class OrganizationConfiguration : IEntityTypeConfiguration<Organization>
{
    public void Configure(EntityTypeBuilder<Organization> builder)
    {
        builder.ToTable("Organizations");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.Email)
            .IsRequired()
            .HasMaxLength(320);

        builder.Property(x => x.CreatedAtUtc)
            .IsRequired();

        builder.Property(x => x.UpdatedAtUtc)
            .IsRequired();

        builder.Property(x => x.ImportantWorkTimes)
            .HasColumnType("nvarchar(max)")
            .HasConversion(new JsonColumnConverter<List<TimeOnly>>(OrganizationPlanningDefaults.ImportantWorkTimes.ToList()));

        builder.Property(x => x.OpeningHours)
            .HasColumnType("nvarchar(max)")
            .HasConversion(new JsonColumnConverter<List<DayOpeningHours>>([]));

        builder.HasIndex(x => x.Email)
            .IsUnique();
    }
}
