using Company.Service.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Company.Service.Application.Common.Interfaces.Persistence;

public interface IApplicationDbContext
{
    DbSet<Account> Accounts { get; set; }

    DbSet<AccountOrder> AccountOrders { get; set; }

    DbSet<InvoiceAdress> InvoiceAdresses { get; set; }

    DbSet<Subscription> Subscriptions { get; set; }

    DatabaseFacade Database { get; }

    EntityEntry<TEntity> Entry<TEntity>(TEntity entity) where TEntity : class;

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
