using Company.Service.Application.Common.Interfaces.Persistence;
using Company.Service.Infrastructure.Data.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Company.Service.Application.UnitTests;

public class TestDbContext : ServiceDomainPlaceholderDbContext, IApplicationDbContext
{
    public TestDbContext()
        : base()
    {
    }

    public TestDbContext(DbContextOptions options)
        : base(options)
    {
    }
}
