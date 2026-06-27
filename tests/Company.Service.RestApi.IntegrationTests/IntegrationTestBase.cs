using Company.Service.Infrastructure.Data.Persistence;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Time.Testing;
using Respawn;

namespace Company.Service.RestApi.IntegrationTests;

[Collection(IntegrationTestsCollection.CollectionName)]
public abstract class IntegrationTestBase : IClassFixture<IntegrationTestWebAppFactory>, IAsyncLifetime
{
    private readonly IServiceScope _scope;
    protected HttpClient Client;
    internal readonly ServiceDomainPlaceholderDbContext DbContext;
    protected readonly MassTransitTestHarness MassTransitTestHarness;
    protected readonly FakeTimeProvider FakeTimeProvider;

    private readonly string _connectionString;

    public IntegrationTestBase(IntegrationTestWebAppFactory factory)
    {
        _scope = factory.Services.CreateScope();

        Client = factory.CreateClient();

        DbContext = _scope.ServiceProvider.GetRequiredService<ServiceDomainPlaceholderDbContext>();
        MassTransitTestHarness = _scope.ServiceProvider.GetRequiredService<MassTransitTestHarness>();
        FakeTimeProvider = (_scope.ServiceProvider.GetRequiredService<TimeProvider>() as FakeTimeProvider)!;

        _connectionString = factory.GetConnectionString();
    }

    public virtual Task InitializeAsync()
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