using System;
using Microsoft.Extensions.DependencyInjection;

namespace IdentityService.Domain.Tests.Integration;

public class IntegrationTestBase(IntegrationTestApplicationFactory factory) : IClassFixture<IntegrationTestApplicationFactory>, IDisposable
{
    protected readonly IServiceScope ServiceScope = factory.Services.CreateScope();

    public void Dispose()
    {
        ServiceScope?.Dispose();
        GC.SuppressFinalize(this);
    }
}
