using IdentityService.Domain.Users.Entities;
using IdentityService.Infrastructure.Options;
using IdentityService.Infrastructure.Workers;
using IdentityService.Persistence.Contexts;
using IdentityService.Persistence.Options;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace IdentityService.Infrastructure.Injection;

public static class InfrastructureInjector
{
    public static IServiceCollection AddInfrastructureDependencies(this IServiceCollection services,
        IConfiguration configuration)
    {
        string? connectionString = configuration.GetConnectionString(ConnectionStringOptions.SectionName);
        ArgumentException.ThrowIfNullOrWhiteSpace(nameof(connectionString));

        RebusOptions? rebusOptions = configuration.GetSection(RebusOptions.SectionName)
            .Get<RebusOptions>();
        ArgumentNullException.ThrowIfNull(rebusOptions);

        services.AddIdentity<ApplicationUser, ApplicationRole>()
            .AddEntityFrameworkStores<ApplicationContext>()
            .AddDefaultTokenProviders();

        services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
            .AddCookie();

        // services.AddRebus(configure => configure
        //         .Routing(r => r.TypeBased()
        //             .Map<UserRegistrationStartedEvent>(rebusOptions.UserCreatedQueue))
        //         .Sagas(t => t.StoreInPostgres(
        //             connectionString: connectionString,
        //             "sagas",
        //             "saga_index"
        //         ))
        //         .Options(options =>
        //         {
        //             options.SetNumberOfWorkers(1);
        //             options.SetMaxParallelism(1);
        //         })
        // );

        services.AddOpenIddict()
            .AddCore(options =>
            {
                options.UseEntityFrameworkCore()
                    .UseDbContext<ApplicationContext>();
            })
            .AddServer(options =>
            {
                options.SetTokenEndpointUris("/connect/token")
                    .SetAuthorizationEndpointUris("/connect/authorize");

                options.AllowClientCredentialsFlow();
                options.AllowAuthorizationCodeFlow();

                options.AddDevelopmentEncryptionCertificate()
                    .AddDevelopmentSigningCertificate();

                options.UseAspNetCore()
                    .DisableTransportSecurityRequirement() // TODO[#5]: Remove this in production
                    .EnableAuthorizationEndpointPassthrough()
                    .EnableTokenEndpointPassthrough();
            })
            .AddValidation(options =>
            {
                options.UseLocalServer();
                options.UseAspNetCore();
            });

        services.AddHostedService<ClientWorker>();

        return services;
    }
}
