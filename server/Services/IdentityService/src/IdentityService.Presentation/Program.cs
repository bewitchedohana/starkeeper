using IdentityService.Infrastructure.Injection;
using IdentityService.Persistence.Injection;
using IdentityService.Application.Injection;
using IdentityService.Presentation.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();

builder.Services.AddPersistenceDependencies(builder.Configuration)
    .AddInfrastructureDependencies(builder.Configuration)
    .AddApplicationDependencies();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    // TODO[#4] Add implementation for error page
    app.UseExceptionHandler("/Error");
}

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();
app.MapDefaultControllerRoute();
app.MapRazorPages()
   .WithStaticAssets();

app.MigrateDatabase();

app.Run();
public partial class Program { }