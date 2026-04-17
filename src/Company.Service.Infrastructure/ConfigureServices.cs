using Company.Service.Application.Common.Interfaces.Persistence;
using Company.Service.Infrastructure.Data.Persistence;
using Company.Service.Infrastructure.Messaging;
using Company.Service.Infrastructure.Persistence;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Company.Service.Infrastructure;

public static class ConfigureServices
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton(TimeProvider.System);

        services.AddMessaging(configuration);

        services.AddDbContext<ServiceDomainPlaceholderDbContext>((sp, optionsBuilder) =>
        {
            var connectionString = configuration.GetConnectionString("ServiceDomainPlaceholderDb")!;
            optionsBuilder.UseSqlServer(connectionString);
        });

        services.AddScoped<IApplicationDbContext, ApplicationDbContext>();

        services.AddMemoryCache();

        return services;
    }
}