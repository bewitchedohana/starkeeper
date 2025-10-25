using IdentityService.Persistence.Contexts;
using IdentityService.Persistence.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace IdentityService.Persistence.Injection;

public static class PersistenceInjector
{
    public static IServiceCollection AddPersistenceDependencies(this IServiceCollection services,
        IConfiguration configuration)
    {
        IConfigurationSection connectionStringSection = configuration.GetSection(ConnectionStringOptions.SectionName)
            ?? throw new InvalidOperationException("ConnectionStringOptions not found.");

        ConnectionStringOptions connectionStringOptions = connectionStringSection
            .Get<ConnectionStringOptions>()
            ?? throw new InvalidOperationException("Failed to bind ConnectionStringOptions.");

        services.Configure<ConnectionStringOptions>(connectionStringSection);

        services.AddDbContext<ApplicationContext>(options =>
        {
            options.UseNpgsql(connectionStringOptions.IdentityServiceContext);
            options.UseOpenIddict();
            options.UseSnakeCaseNamingConvention();
        });
        
        return services;
    }
}
