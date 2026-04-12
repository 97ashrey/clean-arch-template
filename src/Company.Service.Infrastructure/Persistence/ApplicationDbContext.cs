using Company.Service.Application.Common.Interfaces.Persistence;
using Company.Service.Domain.Common;
using Company.Service.Domain.Entities;
using Company.Service.Infrastructure.Data.Persistence;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Company.Service.Infrastructure.Persistence;

internal class ApplicationDbContext : IApplicationDbContext
{
    private readonly ServiceDomainPlaceholderDbContext _dbContext;
    private readonly IPublishEndpoint _publishEndpoint;

    public ApplicationDbContext(ServiceDomainPlaceholderDbContext dbContext, IPublishEndpoint publishEndpoint)
    {
        _dbContext = dbContext;
        _publishEndpoint = publishEndpoint;
    }

    public DbSet<Account> Accounts  => _dbContext.Accounts;
    public DbSet<AccountOrder> AccountOrders => _dbContext.AccountOrders;
    public DbSet<InvoiceAdress> InvoiceAdresses  => _dbContext.InvoiceAdresses;
    public DbSet<Subscription> Subscriptions  => _dbContext.Subscriptions;

    public DatabaseFacade Database => _dbContext.Database;

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var entries = _dbContext.ChangeTracker.Entries<Entity>().ToList();

        foreach (var entry in entries)
        {
            var entity = entry.Entity;
            var domainEvents = entity.DomainEvents;

            foreach (var domainEvent in domainEvents)
            {
                await _publishEndpoint.Publish(domainEvent, cancellationToken);
            }

            entity.ClearDomainEvents();
        }

        return await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
