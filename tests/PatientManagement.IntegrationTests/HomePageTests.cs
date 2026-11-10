using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;

namespace PatientManagement.IntegrationTests;

// Infrastructure tests: the app starts with its configuration. The home page now needs a signed-in user
// (NFR-SEC-001, TC-NFR-SEC-001-1), so these tests check the redirect instead of the page content.
public class HomePageTests(WebApplicationFactory<Program> factory) : IClassFixture<WebApplicationFactory<Program>>
{
    [Fact]
    public async Task Home_page_redirects_to_login_when_signed_out()
    {
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

        var response = await client.GetAsync("/");

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.NotNull(response.Headers.Location);
        var location = new Uri(new Uri("http://localhost"), response.Headers.Location);
        Assert.Equal("/Account/Login", location.AbsolutePath);
    }
}
