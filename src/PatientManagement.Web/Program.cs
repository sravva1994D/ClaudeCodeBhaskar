using PatientManagement.Business;
using PatientManagement.Data.Context;
using PatientManagement.Web.Security;

var builder = WebApplication.CreateBuilder(args);

// Layer registration (CLAUDE.md §6). Web references Data only for AddDataLayer.
builder.Services.AddBusinessLayer();
builder.Services.AddDataLayer(builder.Configuration);

// Sign-in cookie, 24-hour sliding timeout and global authorization (NFR-SEC-001).
builder.Services.AddPatientManagementSecurity();

builder.Services.AddControllersWithViews();

var app = builder.Build();

app.Logger.LogInformation("PatientManagement.Web starting in {Environment} environment.", app.Environment.EnvironmentName);

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets().AllowAnonymous();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();

// Exposes the entry point to PatientManagement.IntegrationTests.
public partial class Program { }
