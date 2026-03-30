using Company.Service.Infrastructure.Data.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddDbContext<ServiceDomainPlaceholderDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("ServiceDomainPlaceholderDb"),
        b => b.MigrationsAssembly(typeof(Program).Assembly.FullName));
});

using IHost host = builder.Build();

using var scope = host.Services.CreateScope();

var logger = scope.ServiceProvider.GetService<ILogger<Program>>();
await using var dbContext = scope.ServiceProvider.GetService<ServiceDomainPlaceholderDbContext>();

try
{
    await dbContext!.Database.MigrateAsync();
}
catch (Exception ex)
{
    logger!.LogError(ex, "An error occurred while applying migrations!");
    Environment.Exit(-1);
}