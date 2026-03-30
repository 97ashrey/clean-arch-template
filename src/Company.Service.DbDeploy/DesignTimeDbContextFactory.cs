using Company.Service.Infrastructure.Data.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Company.Service.DbDeploy;

public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<ServiceDomainPlaceholderDbContext>
{
    public ServiceDomainPlaceholderDbContext CreateDbContext(string[] args)
    {
        var builder = new DbContextOptionsBuilder<ServiceDomainPlaceholderDbContext>();
        var connectionString = "Server=127.0.0.1,52160;User ID=sa;Password=P@5sword;TrustServerCertificate=true;Initial Catalog=ServiceDomainPlaceholderDb";
        
        builder.UseSqlServer(connectionString,
            b => b.MigrationsAssembly(typeof(DesignTimeDbContextFactory).Assembly.FullName));
        
        return new ServiceDomainPlaceholderDbContext(builder.Options);
    }
}