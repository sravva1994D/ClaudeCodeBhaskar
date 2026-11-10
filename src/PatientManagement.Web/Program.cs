using PatientManagement.Business;
using PatientManagement.Data.Context;

var builder = WebApplication.CreateBuilder(args);

// Layer registration (CLAUDE.md §6). Web references Data only for AddDataLayer.
builder.Services.AddBusinessLayer();
builder.Services.AddDataLayer(builder.Configuration);

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

// Authentication is added with the login feature (BRD: Security, NFR-SEC-001).
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();

// Exposes the entry point to PatientManagement.IntegrationTests.
public partial class Program { }
