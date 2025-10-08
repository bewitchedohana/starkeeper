using IdentityService.Infrastructure.Injection;
using IdentityService.Persistence.Injection;
using IdentityService.Application.Injection;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();

builder.Services.AddPersistenceDependencies(builder.Configuration)
    .AddInfrastructureDependencies()
    .AddApplicationDependencies();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    // TODO[#4] Add implementation for error page
    app.UseExceptionHandler("/Error");
}

app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();
app.MapDefaultControllerRoute();
app.MapRazorPages()
   .WithStaticAssets();

app.Run();
