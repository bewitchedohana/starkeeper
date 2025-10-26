using IdentityService.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;

namespace IdentityService.Presentation.Extensions;

public static class WebApplicationExtensions
{
    public static void MigrateDatabase(this WebApplication webApplication)
    {
        using IServiceScope scope = webApplication.Services.CreateScope();
        ApplicationContext context = scope.ServiceProvider
            .GetRequiredService<ApplicationContext>();

        context.Database.Migrate();
    }
}
