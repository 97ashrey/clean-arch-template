//__EXAMPLE_START__
using Company.Service.Domain.Entities;
//__EXAMPLE_END__

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Company.Service.Application.Common.Interfaces.Persistence;

public interface IApplicationDbContext
{
#if EXAMPLE
    DbSet<Account> Accounts { get; }

    DbSet<AccountOrder> AccountOrders { get; }

    DbSet<Subscription> Subscriptions { get; }
#endif

    //__EXAMPLE_START__
    DbSet<InvoiceAddress> InvoiceAdresses { get; }
    //__EXAMPLE_END__

    DatabaseFacade Database { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}