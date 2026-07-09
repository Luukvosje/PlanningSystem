using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Planning.Domain.Invites;
using Planning.Domain.Organizations;

namespace Planning.Infrastructure.Invites;

public class OrganizationInviteConfiguration : IEntityTypeConfiguration<OrganizationInvite>
{
    public void Configure(EntityTypeBuilder<OrganizationInvite> builder)
    {
        builder.ToTable("OrganizationInvites");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.OrganizationId)
            .IsRequired();

        builder.Property(x => x.Code)
            .IsRequired()
            .HasMaxLength(16);

        builder.Property(x => x.Role)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(x => x.CreatedByUserId)
            .IsRequired();

        builder.Property(x => x.ExpiresAtUtc)
            .IsRequired();

        builder.Property(x => x.CreatedAtUtc)
            .IsRequired();

        builder.Property(x => x.UpdatedAtUtc)
            .IsRequired();

        builder.HasIndex(x => x.Code)
            .IsUnique();

        builder.HasIndex(x => x.OrganizationId);

        builder.HasOne<Organization>()
            .WithMany()
            .HasForeignKey(x => x.OrganizationId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
