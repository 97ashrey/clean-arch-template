using Company.Service.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Company.Service.Infrastructure.Data.Persistence.EntityConfigurations;

internal class SubscriptionEntityConfiguration : IEntityTypeConfiguration<Subscription>
{
    public void Configure(EntityTypeBuilder<Subscription> builder)
    {
        builder.HasKey(s => s.Id);

        builder.Property(s => s.Name)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(s => s.FriendlyName)
            .IsRequired()
            .HasMaxLength(255);

        builder.OwnsOne(s => s.PurchasePrice, priceBuilder =>
        {
            priceBuilder.Property(p => p.Value)
                .IsRequired();

            priceBuilder.Property(p => p.Currency)
                .IsRequired()
                .HasMaxLength(3);
        });

        builder.Property(s => s.BillCycle)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(30);

        builder.Property(s => s.StartDate)
            .IsRequired();

        builder.Property(s => s.EndDate)
            .IsRequired();

        builder.Property(s => s.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(30);

        builder.Property(s => s.SuspendedDate)
            .IsRequired(false);

        builder.Property(s => s.ProductId)
            .IsRequired();

        builder.HasOne<Account>()
            .WithMany()
            .HasForeignKey(s => s.AccountId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();

        builder.ToTable("Subscriptions");
    }
}
