using Company.Service.Infrastructure.Data.Persistence;
using Mediator;
using Microsoft.Extensions.DependencyInjection;
using Respawn;

namespace Company.Service.RestApi.IntegrationTests;

[Collection(IntegrationTestsCollection.CollectionName)]
public abstract class IntegrationTestBase : IClassFixture<IntegrationTestWebAppFactory>, IAsyncLifetime
{
    private readonly IServiceScope _scope;
    protected readonly ISender Sender;
    protected HttpClient Client;
    internal readonly ServiceDomainPlaceholderDbContext DbContext;
    protected readonly MassTransitTestHarness MassTransitTestHarness;

    private readonly string _connectionString;

    public IntegrationTestBase(IntegrationTestWebAppFactory factory)
    {
        _scope = factory.Services.CreateScope();
        Client = factory.CreateClient();
        Sender = _scope.ServiceProvider.GetRequiredService<ISender>();
        DbContext = _scope.ServiceProvider.GetRequiredService<ServiceDomainPlaceholderDbContext>();
        MassTransitTestHarness = _scope.ServiceProvider.GetRequiredService<MassTransitTestHarness>();
        _connectionString = factory.GetConnectionString();
    }

    public Task InitializeAsync()
    {
        return Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        Client?.Dispose();

        await ResetDatabaseAsync();

        DbContext?.Dispose();
        _scope?.Dispose();
    }

    private async Task ResetDatabaseAsync()
    {
        var conn = new Microsoft.Data.SqlClient.SqlConnection(_connectionString);
        await conn.OpenAsync();

        var respawner = await Respawner.CreateAsync(
            conn,
            new RespawnerOptions
            {
                DbAdapter = DbAdapter.SqlServer,
            }
        );

        await respawner.ResetAsync(conn);

        await conn.CloseAsync();
        await conn.DisposeAsync();
    }

}