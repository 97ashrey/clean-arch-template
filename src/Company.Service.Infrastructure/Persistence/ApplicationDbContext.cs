using Company.Service.Application.Common.Interfaces.Persistence;
//__EXAMPLE_START__
using Company.Service.Domain.Entities;
//__EXAMPLE_END__
using Company.Service.Infrastructure.Data.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Company.Service.Infrastructure.Persistence;

internal class ApplicationDbContext : IApplicationDbContext
{
    private readonly ServiceDomainPlaceholderDbContext _dbContext;

    public ApplicationDbContext(ServiceDomainPlaceholderDbContext dbContext)
    {
        _dbContext = dbContext;
    }

#if EXAMPLE
    public DbSet<Account> Accounts => _dbContext.Accounts;

    public DbSet<AccountOrder> AccountOrders => _dbContext.AccountOrders;

    public DbSet<Subscription> Subscriptions => _dbContext.Subscriptions;
#endif

    //__EXAMPLE_START__
    public DbSet<InvoiceAddress> InvoiceAdresses => _dbContext.InvoiceAdresses;
    //__EXAMPLE_END__

    public DatabaseFacade Database => _dbContext.Database;

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Potential pre save logic can go here
        return await _dbContext.SaveChangesAsync(cancellationToken);
    }
}