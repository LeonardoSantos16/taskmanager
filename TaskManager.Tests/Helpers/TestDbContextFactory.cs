using Microsoft.EntityFrameworkCore;
using taskmanager.Context;

namespace TaskManager.Tests.Helpers;

public static class TestDbContextFactory
{
    public static AppDbContext CreateInMemory()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(options);
    }
}
