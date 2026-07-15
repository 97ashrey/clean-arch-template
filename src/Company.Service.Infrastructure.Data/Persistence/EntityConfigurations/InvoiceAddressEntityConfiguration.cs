//__EXAMPLE_START__
using Company.Service.Domain.Entities;
using Company.Service.Infrastructure.Data.Persistence.ValueConverters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Company.Service.Infrastructure.Data.Persistence.EntityConfigurations;

internal class InvoiceAddressEntityConfiguration : IEntityTypeConfiguration<InvoiceAddress>
{
    public void Configure(EntityTypeBuilder<InvoiceAddress> builder)
    {
        builder.HasKey(ia => ia.Id);

        builder.Property(ia => ia.Id)
            .HasColumnType("binary(16)")
            .HasConversion<GuidValueConverter>();

        builder.Property(ia => ia.TenantId)
            .IsRequired();

        builder.Property(ia => ia.Name)
            .IsRequired()
            .HasMaxLength(255);

        builder.OwnsOne(ia => ia.Address, addressBuilder =>
        {
            addressBuilder.Property(a => a.Country)
                .IsRequired()
                .HasMaxLength(100);

            addressBuilder.Property(a => a.City)
                .IsRequired()
                .HasMaxLength(100);

            addressBuilder.Property(a => a.ZipCode)
                .IsRequired()
                .HasMaxLength(20);

            addressBuilder.Property(a => a.Street)
                .IsRequired()
                .HasMaxLength(255);

            addressBuilder.Property(a => a.Number)
                .IsRequired()
                .HasMaxLength(50);
        });

        builder.ToTable("InvoiceAddresses");
    }
}
//__EXAMPLE_END__