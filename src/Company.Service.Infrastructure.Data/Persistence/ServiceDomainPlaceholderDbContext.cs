using Company.Service.Domain.Entities;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace Company.Service.Infrastructure.Data.Persistence;

public class ServiceDomainPlaceholderDbContext : DbContext
{
    public DbSet<Account> Accounts { get; set; }

    public DbSet<AccountOrder> AccountOrders { get; set; }

    public DbSet<InvoiceAdress> InvoiceAdresses { get; set; }

    public DbSet<Subscription> Subscriptions { get; set; }

    public ServiceDomainPlaceholderDbContext()
        : base()
    {
    }

    public ServiceDomainPlaceholderDbContext(DbContextOptions options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ServiceDomainPlaceholderDbContext).Assembly);

        modelBuilder.AddInboxStateEntity();
        modelBuilder.AddOutboxMessageEntity();
        modelBuilder.AddOutboxStateEntity();
    }
}
