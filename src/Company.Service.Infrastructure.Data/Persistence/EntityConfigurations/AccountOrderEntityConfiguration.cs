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

        builder.Property(ao => ao.CompletedDate)
            .IsRequired(false);

        builder.OwnsOne(ao => ao.AccountDetails, ad =>
        {
            ad.Property(a => a.Name)
                .IsRequired()
                .HasMaxLength(255);

            ad.Property(a => a.Tier)
                .IsRequired()
                .HasConversion<string>()
                .HasMaxLength(30);

            ad.Property(a => a.InvoiceAddressId)
                .IsRequired();

            ad.HasOne<InvoiceAdress>()
                .WithMany()
                .HasForeignKey(a => a.InvoiceAddressId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired();
        });

        builder.ToTable("AccountOrders");
    }
}