extern alias TaskManagerApi;

using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using taskmanager.Context;
using TaskManager.Tests.Helpers;

namespace TaskManager.Tests.Integration;

[Collection(MsSqlCollection.Name)]
public abstract class IntegrationTestBase : IAsyncLifetime
{
    private readonly MsSqlContainerFixture _sqlFixture;
    private CustomWebApplicationFactory _factory = null!;
    protected HttpClient Client { get; private set; } = null!;

    protected IntegrationTestBase(MsSqlContainerFixture sqlFixture)
    {
        _sqlFixture = sqlFixture;
    }

    public async Task InitializeAsync()
    {
        var connectionStringBuilder = new SqlConnectionStringBuilder(_sqlFixture.Container.GetConnectionString())
        {
            InitialCatalog = $"IntegrationTests_{Guid.NewGuid():N}"
        };

        _factory = new CustomWebApplicationFactory(connectionStringBuilder.ConnectionString);
        Client = _factory.CreateClient();

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await db.Database.MigrateAsync();
    }

    public async Task DisposeAsync()
    {
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await db.Database.EnsureDeletedAsync();
        }

        Client.Dispose();
        await _factory.DisposeAsync();
    }

    protected HttpClient CreateAdditionalClient() => _factory.CreateClient();
}
