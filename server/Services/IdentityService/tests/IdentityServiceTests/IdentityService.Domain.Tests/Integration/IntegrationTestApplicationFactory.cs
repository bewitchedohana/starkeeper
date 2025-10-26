using IdentityService.Persistence.Contexts;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Testcontainers.PostgreSql;

namespace IdentityService.Domain.Tests.Integration;

public class IntegrationTestApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgresqlContainer = new PostgreSqlBuilder()
        .WithImage("postgres:16")
        .WithPassword("test")
        .Build();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<ApplicationContext>();
            services.AddDbContext<ApplicationContext>(options =>
                options.UseNpgsql(_postgresqlContainer.GetConnectionString()));
        });
    }

    public Task InitializeAsync()
    {
        return _postgresqlContainer.StartAsync();
    }

    Task IAsyncLifetime.DisposeAsync()
    {
        return _postgresqlContainer.StopAsync();
    }
}
