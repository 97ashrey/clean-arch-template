//__EXAMPLE_START__
using Company.Service.Domain.Entities;
//__EXAMPLE_END__

using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace Company.Service.Infrastructure.Data.Persistence;

public class ServiceDomainPlaceholderDbContext : DbContext
{
#if EXAMPLE
    public DbSet<Account> Accounts { get; set; }

    public DbSet<AccountOrder> AccountOrders { get; set; }

    public DbSet<Subscription> Subscriptions { get; set; }
#endif

    //__EXAMPLE_START__
    public DbSet<InvoiceAddress> InvoiceAdresses { get; set; }
    //__EXAMPLE_END__

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