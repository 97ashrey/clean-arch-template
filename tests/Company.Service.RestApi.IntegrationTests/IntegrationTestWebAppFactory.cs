using Company.Service.Infrastructure.Data.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Time.Testing;

namespace Company.Service.RestApi.IntegrationTests;

public class IntegrationTestWebAppFactory : WebApplicationFactory<Program>
{
    private readonly TestContainerFixture _testContainerFixture;
    private readonly MassTransitTestHarness _massTransitTestHarness = new();

    public IntegrationTestWebAppFactory(TestContainerFixture testContainerFixture)
    {
        _testContainerFixture = testContainerFixture;
    }

    protected override IHost CreateHost(IHostBuilder builder)
    {
        builder.ConfigureHostConfiguration(config =>
        {
            config.AddInMemoryCollection(
             new Dictionary<string, string?>
             {
                { "Authority:Enabled", "false" },
                { "ExportTelemetry", "false" },
                { "ConnectionStrings:MessagingBus", null }
             }
            );
        });

        return base.CreateHost(builder);
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureTestServices(services =>
        {
            var connectionString = _testContainerFixture.DbContainer.GetConnectionString();
            services.RemoveAll<DbContextOptions<ServiceDomainPlaceholderDbContext>>();

            services.AddDbContext<ServiceDomainPlaceholderDbContext>(options =>
            {
                options.UseSqlServer(connectionString);
            });

            services.AddSingleton(_massTransitTestHarness);

            services.AddSingleton(provider =>
            {
                _massTransitTestHarness.InitializeAsync().GetAwaiter().GetResult();
                return _massTransitTestHarness.PublishEndpoint;
            });

            // Replace TimeProvider.System with a controllable FakeTimeProvider
            services.RemoveAll<TimeProvider>();
            services.AddSingleton<TimeProvider>(new FakeTimeProvider());
        });
    }

    public string GetConnectionString()
    {
        return _testContainerFixture.DbContainer.GetConnectionString();
    }
}