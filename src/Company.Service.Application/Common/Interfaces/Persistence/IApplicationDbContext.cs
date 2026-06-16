using Company.Service.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Company.Service.Application.Common.Interfaces.Persistence;

public interface IApplicationDbContext
{
    DbSet<Account> Accounts { get; }

    DbSet<AccountOrder> AccountOrders { get; }

    DbSet<InvoiceAddress> InvoiceAdresses { get; }

    DbSet<Subscription> Subscriptions { get; }

    DatabaseFacade Database { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}