using IdentityService.Persistence.Contexts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenIddict.Abstractions;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace IdentityService.Infrastructure.Workers;

public class ClientWorker(IServiceProvider serviceProvider) : IHostedService
{
    private readonly IServiceProvider _serviceProvider = serviceProvider;

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        // TODO: Improve this to match actual initialization logic

        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationContext>();
        await context.Database.EnsureCreatedAsync();

        var manager = scope.ServiceProvider.GetRequiredService<IOpenIddictApplicationManager>();


        // TODO: Adjust client registration
        var client = await manager.FindByClientIdAsync("service-worker");
        if (client is not null)
        {
            await manager.DeleteAsync(client);
        }

        if (true) // TODO: Replace with actual condition to check if the client exists
        {
            await manager.CreateAsync(new OpenIddictApplicationDescriptor
            {
                ClientId = "service-worker",
                ClientSecret = "388D45FA-B36B-4988-BA59-B187D329C207",
                RedirectUris = { new Uri("https://localhost:5056/signin-oidc") },
                Permissions =
                {
                    Permissions.Endpoints.Token,
                    Permissions.Endpoints.Authorization,
                    Permissions.GrantTypes.ClientCredentials,
                    Permissions.GrantTypes.AuthorizationCode,
                    Permissions.ResponseTypes.Code
                }
            });
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
