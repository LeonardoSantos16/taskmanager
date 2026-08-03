using Testcontainers.MsSql;

namespace TaskManager.Tests.Helpers;

public class MsSqlContainerFixture : IAsyncLifetime
{
    public MsSqlContainer Container { get; } =
        new MsSqlBuilder("mcr.microsoft.com/mssql/server:2022-latest").Build();

    public Task InitializeAsync() => Container.StartAsync();

    public Task DisposeAsync() => Container.DisposeAsync().AsTask();
}

[CollectionDefinition(Name)]
public class MsSqlCollection : ICollectionFixture<MsSqlContainerFixture>
{
    public const string Name = "MsSql collection";
}
