using System.Reflection;
using IdentityService.Application.Abstractions;
using IdentityService.Application.Features.Users.CreateUserCommand;
using Mediator;
using Microsoft.Extensions.DependencyInjection;

namespace IdentityService.Application.Injection;

public static class ApplicationInjection
{
    public static IServiceCollection AddApplicationDependencies(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddMediator(options =>
        {
            options.Assemblies = [typeof(ApplicationAssemblyReference)];
            options.ServiceLifetime = ServiceLifetime.Scoped;
            options.PipelineBehaviors = [typeof(CreateUserCommandPipeline)];
        });

        return serviceCollection;
    }
}
