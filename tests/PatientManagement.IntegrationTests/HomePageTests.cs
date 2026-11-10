using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;

namespace PatientManagement.IntegrationTests;

// Infrastructure test: the application starts with its configuration and serves the placeholder home page.
// It does not connect to SQL Server. It verifies no BRD requirement.
public class HomePageTests(WebApplicationFactory<Program> factory) : IClassFixture<WebApplicationFactory<Program>>
{
    [Fact]
    public async Task Home_page_responds_with_success()
    {
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Home_page_shows_the_skeleton_banner()
    {
        using var client = factory.CreateClient();

        var html = await client.GetStringAsync("/");

        Assert.Contains("Phase 1 solution skeleton", html);
    }
}
