using PatientManagement.Business;
using PatientManagement.Data.Context;
using PatientManagement.Web.Security;
using PatientManagement.Web.Setup;

var builder = WebApplication.CreateBuilder(args);

// Layer registration (CLAUDE.md §6). Web references Data only for AddDataLayer.
builder.Services.AddBusinessLayer();
builder.Services.AddDataLayer(builder.Configuration);

// Sign-in cookie, 24-hour sliding timeout and global authorization (NFR-SEC-001).
builder.Services.AddPatientManagementSecurity();

// Console seed command (DEC-018). Used only when the first argument is seed-user.
builder.Services.AddSingleton<IConsoleIO, SystemConsoleIO>();
builder.Services.AddTransient<SeedUserRunner>();

builder.Services.AddControllersWithViews();

var app = builder.Build();

if (args.Length > 0 && args[0] == SeedUserRunner.CommandName)
{
    using var scope = app.Services.CreateScope();
    return await scope.ServiceProvider.GetRequiredService<SeedUserRunner>().RunAsync();
}

app.Logger.LogInformation("PatientManagement.Web starting in {Environment} environment.", app.Environment.EnvironmentName);

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// Plain HTTP is allowed on localhost in Development (PDR-21). Redirecting there only logs a warning,
// because no HTTPS port is configured for the local profile. Other environments keep the redirect.
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets().AllowAnonymous();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();

return 0;

// Exposes the entry point to PatientManagement.IntegrationTests.
public partial class Program { }
