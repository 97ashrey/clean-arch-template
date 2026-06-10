using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Company.Service.Application.UnitTests;

public class DbContextTestBase : IDisposable
{
    protected readonly SqliteConnection Connection;
    protected readonly TestDbContext DbContext;

    public DbContextTestBase()
    {
        Connection = new SqliteConnection("Data Source=:memory:");
        Connection.Open();

        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseSqlite(Connection, contextOwnsConnection: true)
            .Options;

        DbContext = new TestDbContext(options);

        DbContext.Database.EnsureCreated();
    }

    public void Dispose()
    {
        DbContext?.Dispose();
        GC.SuppressFinalize(this);
    }
}