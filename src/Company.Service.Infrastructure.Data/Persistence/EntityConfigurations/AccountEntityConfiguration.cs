using Company.Service.Domain.Entities;
using Company.Service.Infrastructure.Data.Persistence.ValueConverters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Company.Service.Infrastructure.Data.Persistence.EntityConfigurations;

internal class AccountEntityConfiguration : IEntityTypeConfiguration<Account>
{
    public void Configure(EntityTypeBuilder<Account> builder)
    {
        builder.HasKey(a => a.Id);

        builder.Property(a => a.Id)
            .HasColumnType("binary(16)")
            .HasConversion<GuidValueConverter>();

        builder.Property(a => a.TenantId)
            .IsRequired();

        builder.Property(a => a.Name)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(a => a.Email)
            .IsRequired()
            .HasMaxLength(320);

        builder.Property(a => a.Tier)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(30);

        builder.Property(a => a.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(30);

        builder.Property(a => a.SuspendedDate)
            .IsRequired(false);

        builder.Property(a => a.InvoiceAddressId)
            .HasColumnType("binary(16)")
            .HasConversion<GuidValueConverter>()
            .IsRequired();

        builder.HasOne<InvoiceAddress>()
            .WithMany()
            .HasForeignKey(a => a.InvoiceAddressId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();

        builder.ToTable("Accounts");
    }
}