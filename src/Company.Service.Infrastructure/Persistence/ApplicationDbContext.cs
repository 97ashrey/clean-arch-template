using Company.Service.Application.Common.Interfaces.Persistence;
using Company.Service.Infrastructure.Data.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Company.Service.Infrastructure.Persistence;

public class ApplicationDbContext : ServiceDomainPlaceholderDbContext, IApplicationDbContext
{
    public ApplicationDbContext()
            : base()
    {
    }

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }
}
