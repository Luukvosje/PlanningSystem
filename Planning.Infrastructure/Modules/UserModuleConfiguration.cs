using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Planning.Domain.Modules;
using Planning.Domain.Users;

namespace Planning.Infrastructure.Modules;

public class UserModuleConfiguration : IEntityTypeConfiguration<UserModule>
{
    public void Configure(EntityTypeBuilder<UserModule> builder)
    {
        builder.ToTable("UserModules");

        builder.HasKey(x => new { x.UserId, x.Module });

        builder.Property(x => x.Module)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(x => x.IsEnabled)
            .IsRequired();

        builder.HasIndex(x => x.UserId);

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
