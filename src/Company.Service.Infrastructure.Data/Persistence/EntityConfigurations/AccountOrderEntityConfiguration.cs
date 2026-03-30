using Company.Service.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Company.Service.Infrastructure.Data.Persistence.EntityConfigurations;

internal class AccountOrderEntityConfiguration : IEntityTypeConfiguration<AccountOrder>
{
    public void Configure(EntityTypeBuilder<AccountOrder> builder)
    {
        builder.HasKey(ao => ao.Id);

        builder.Property(a => a.TenantId)
            .IsRequired();

        builder.Property(ao => ao.AccountName)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(ao => ao.Tier)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(30);

        builder.OwnsOne(ao => ao.ContactInformation, ci =>
        {
            ci.Property(c => c.FirstName)
                .IsRequired()
                .HasMaxLength(100);

            ci.Property(c => c.LastName)
                .IsRequired()
                .HasMaxLength(100);

            ci.Property(c => c.Email)
                .IsRequired()
                .HasMaxLength(320);

            ci.Property(c => c.PhoneNumber)
                .IsRequired()
                .HasMaxLength(20);
        });

        builder.Property(ao => ao.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(30);

        builder.Property(ao => ao.CreatedDate)
            .IsRequired();

        builder.HasOne<InvoiceAdress>()
            .WithMany()
            .HasForeignKey(ao => ao.InvoiceAddressId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();

        builder.ToTable("AccountOrders");
    }
}
