using Company.Service.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Company.Service.Infrastructure.Data.Persistence.EntityConfigurations;

internal class InvoiceAddressEntityConfiguration : IEntityTypeConfiguration<InvoiceAdress>
{
    public void Configure(EntityTypeBuilder<InvoiceAdress> builder)
    {
        builder.HasKey(ia => ia.Id);

        builder.Property(ia => ia.TenantId)
            .IsRequired();

        builder.Property(ia => ia.Name)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(ia => ia.Country)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(ia => ia.City)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(ia => ia.ZipCode)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(ia => ia.Street)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(ia => ia.Number)
            .IsRequired()
            .HasMaxLength(50);

        builder.ToTable("InvoiceAddresses");
    }
}
