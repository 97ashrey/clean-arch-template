using Company.Service.Application;
using Company.Service.Infrastructure.Data.Persistence;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Company.Service.Infrastructure.Messaging;

internal static class ConfigureMessagingServices
{
    public static IServiceCollection AddMessaging(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddMassTransit(x =>
        {
            x.AddEntityFrameworkOutbox<ServiceDomainPlaceholderDbContext>(o =>
            {
               o.QueryDelay = TimeSpan.FromSeconds(60);
               o.UseSqlServer();
               o.UseBusOutbox();
            });

            x.AddConsumersFromNamespaceContaining<IApplicationAssemblyMarker>();

            var bussConnectionString = configuration.GetConnectionString("MessagingBus");

            if (string.IsNullOrWhiteSpace(bussConnectionString))
            {
                x.ConfigureInMemoryBus();
            }
            else
            {
                x.ConfigureRabitMq(bussConnectionString);
            }
        });

        return services;
    }

    private static void ConfigureInMemoryBus(this IBusRegistrationConfigurator configurator)
    {
        configurator.AddDelayedMessageScheduler();
        configurator.UsingInMemory((context, cfg) =>
        {
            cfg.UseDelayedMessageScheduler();
            cfg.ConfigureEndpoints(context);
        });
    }

    private static void ConfigureRabitMq(this IBusRegistrationConfigurator configurator, string connectionString)
    {
        configurator.AddDelayedMessageScheduler();
        configurator.UsingRabbitMq((context, cfg) =>
        {
            cfg.Host(connectionString);

            cfg.UseDelayedMessageScheduler();

            cfg.ConfigureEndpoints(context);
        });
    }
}
