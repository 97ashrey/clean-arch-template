using Company.Service.DbDeploy;
using Company.Service.Infrastructure.Data.Persistence;
using Microsoft.EntityFrameworkCore;
using Testcontainers.MsSql;

namespace Company.Service.RestApi.IntegrationTests;

public class TestContainerFixture : IAsyncLifetime
{
    public readonly MsSqlContainer DbContainer = new MsSqlBuilder("mcr.microsoft.com/mssql/server:2022-CU25-ubuntu-22.04").Build();

    public async Task InitializeAsync()
    {
        await DbContainer.StartAsync();
        await EnsureDatabaseUpdated();
    }

    public async Task DisposeAsync()
    {
        await DbContainer.StopAsync();
        await DbContainer.DisposeAsync();
    }

    private async Task EnsureDatabaseUpdated()
    {
        var optionsBuilder = new DbContextOptionsBuilder<ServiceDomainPlaceholderDbContext>()
            .UseSqlServer(
                DbContainer.GetConnectionString(),
                sqloptions => sqloptions.MigrationsAssembly(typeof(DesignTimeDbContextFactory).Assembly.FullName)
            );

        using var dbContext = new ServiceDomainPlaceholderDbContext(optionsBuilder.Options);
        await dbContext.Database.MigrateAsync();
    }
}


[CollectionDefinition(CollectionName)]
public class IntegrationTestsCollection : ICollectionFixture<TestContainerFixture>
{
    public const string CollectionName = "IntegrationTests";
}