using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Planning.Domain.Customers;
using Planning.Domain.Organizations;

namespace Planning.Infrastructure.Customers;



public class CustomerConfiguration : IEntityTypeConfiguration<Customer>

{

    public void Configure(EntityTypeBuilder<Customer> builder)

    {

        builder.ToTable("Customers");



        builder.HasKey(x => x.Id);



        builder.Property(x => x.OrganizationId)

            .IsRequired();



        builder.Property(x => x.Name)

            .IsRequired()

            .HasMaxLength(200);



        builder.Property(x => x.Email)

            .IsRequired()

            .HasMaxLength(320);



        builder.Property(x => x.Address)

            .HasMaxLength(500);



        builder.Property(x => x.CreatedAtUtc)

            .IsRequired();



        builder.Property(x => x.UpdatedAtUtc)

            .IsRequired();



        builder.HasIndex(x => x.OrganizationId);



        builder.HasOne<Organization>()

            .WithMany()

            .HasForeignKey(x => x.OrganizationId)

            .OnDelete(DeleteBehavior.Cascade);

    }

}

