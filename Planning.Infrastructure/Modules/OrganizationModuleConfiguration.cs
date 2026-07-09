using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Planning.Domain.Modules;
using Planning.Domain.Organizations;

namespace Planning.Infrastructure.Modules;

public class OrganizationModuleConfiguration : IEntityTypeConfiguration<OrganizationModule>
{
    public void Configure(EntityTypeBuilder<OrganizationModule> builder)
    {
        builder.ToTable("OrganizationModules");

        builder.HasKey(x => new { x.OrganizationId, x.Module });

        builder.Property(x => x.Module)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(x => x.IsEnabled)
            .IsRequired();

        builder.HasIndex(x => x.OrganizationId);

        builder.HasOne<Organization>()
            .WithMany()
            .HasForeignKey(x => x.OrganizationId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
