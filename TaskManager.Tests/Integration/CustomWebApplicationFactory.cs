extern alias TaskManagerApi;

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using taskmanager.Context;
using ApiProgram = TaskManagerApi::Program;

namespace TaskManager.Tests.Integration;

public class CustomWebApplicationFactory : WebApplicationFactory<ApiProgram>
{
    private readonly string _connectionString;

    public CustomWebApplicationFactory(string connectionString)
    {
        _connectionString = connectionString;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseSetting("Jwt:Key", "integration-tests-only-signing-key-not-used-in-production-0123456789");

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<DbContextOptions<AppDbContext>>();
            services.AddDbContext<AppDbContext>(options => options.UseSqlServer(_connectionString));
        });
    }
}
