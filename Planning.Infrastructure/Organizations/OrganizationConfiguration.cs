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
            .HasColumnType("jsonb")
            .HasConversion(new JsonColumnConverter<List<ImportantWorkTime>>(OrganizationPlanningDefaults.ImportantWorkTimes.ToList()))
            .Metadata.SetValueComparer(new JsonListValueComparer<ImportantWorkTime>());

        builder.Property(x => x.OpeningHours)
            .HasColumnType("jsonb")
            .HasConversion(new JsonColumnConverter<List<DayOpeningHours>>([]))
            .Metadata.SetValueComparer(new JsonListValueComparer<DayOpeningHours>());

        builder.HasIndex(x => x.Email)
            .IsUnique();
    }
}
