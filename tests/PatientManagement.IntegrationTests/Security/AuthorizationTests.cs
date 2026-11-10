using System.Net;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using PatientManagement.Business.Interfaces;
using PatientManagement.Web.Security;

namespace PatientManagement.IntegrationTests.Security;

// Web security tests: global authorization, the sign-in cookie, and the password hasher.
// Test IDs follow CLAUDE.md §4.2 and §10.
public class AuthorizationTests(WebApplicationFactory<Program> factory) : IClassFixture<WebApplicationFactory<Program>>
{
    [Fact]
    public async Task NFR_SEC_001_protected_page_redirects_to_login_with_return_url()
    {
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

        var response = await client.GetAsync("/");

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        // The redirect may be absolute or relative; resolve it against a base before reading the path.
        Assert.NotNull(response.Headers.Location);
        var location = new Uri(new Uri("http://localhost"), response.Headers.Location);
        Assert.Equal("/Account/Login", location.AbsolutePath);
        Assert.Contains("ReturnUrl=", location.Query, StringComparison.Ordinal);
    }

    [Fact]
    public async Task NFR_SEC_001_error_page_is_reachable_when_signed_out()
    {
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

        var response = await client.GetAsync("/Home/Error");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public void NFR_SEC_001_sign_in_cookie_has_the_required_settings()
    {
        var options = factory.Services
            .GetRequiredService<IOptionsMonitor<CookieAuthenticationOptions>>()
            .Get(CookieAuthenticationDefaults.AuthenticationScheme);

        Assert.Equal(SecurityServiceCollectionExtensions.CookieName, options.Cookie.Name);
        Assert.True(options.Cookie.HttpOnly);
        Assert.Equal(Microsoft.AspNetCore.Http.SameSiteMode.Lax, options.Cookie.SameSite);
        Assert.Equal(TimeSpan.FromHours(24), options.ExpireTimeSpan);
        Assert.True(options.SlidingExpiration);
        Assert.Equal(SecurityServiceCollectionExtensions.LoginPath, options.LoginPath);
    }

    [Fact]
    public void NFR_SEC_001_auth_service_and_password_hasher_are_registered()
    {
        using var scope = factory.Services.CreateScope();

        Assert.NotNull(scope.ServiceProvider.GetRequiredService<IAuthService>());
        Assert.NotNull(scope.ServiceProvider.GetRequiredService<IPasswordHasher>());
    }
}
