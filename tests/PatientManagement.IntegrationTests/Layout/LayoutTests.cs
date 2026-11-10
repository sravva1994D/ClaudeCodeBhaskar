using System.Net;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using PatientManagement.Data.Entities;
using PatientManagement.IntegrationTests.Fixtures;
using PatientManagement.Web.Security;

namespace PatientManagement.IntegrationTests.Layout;

// The shared layout: the sign-out control appears only for a signed-in user. Test IDs follow CLAUDE.md §4.2 and §10.
[Collection(TestDatabaseCollection.Name)]
public partial class LayoutTests(TestDatabaseFixture database, TestWebApplicationFactory factory)
    : IClassFixture<TestWebApplicationFactory>, IAsyncLifetime
{
    private const string Username = "test.user";
    private const string Email = "test.user@example.test";
    private const string Password = "Abcdef1!";

    public async Task InitializeAsync()
    {
        await database.ClearTablesAsync("Users");
        await using var context = database.CreateContext();
        context.Users.Add(new User
        {
            Username = Username,
            Email = Email,
            PasswordHash = new AspNetPasswordHasher().Hash(Password)
        });
        await context.SaveChangesAsync();
    }

    public async Task DisposeAsync() => await database.ClearTablesAsync("Users");

    [Fact]
    public async Task NFR_SEC_001_sign_out_control_is_not_shown_on_the_sign_in_page()
    {
        using var client = factory.CreateClient();

        var html = await client.GetStringAsync("/Account/Login");

        Assert.DoesNotContain("Sign out", html);
    }

    [Fact]
    public async Task NFR_SEC_001_signed_in_layout_shows_the_user_name_and_sign_out()
    {
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
        await SignInAsync(client);

        var html = await client.GetStringAsync("/");

        Assert.Contains("Sign out", html);
        Assert.Contains(Username, html);
        Assert.Contains("action=\"/Account/Logout\"", html);
    }

    [Fact]
    public async Task Home_page_shows_the_skeleton_banner_after_sign_in()
    {
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
        await SignInAsync(client);

        var html = await client.GetStringAsync("/");

        Assert.Contains("Phase 1 solution skeleton", html);
    }

    [Fact]
    public async Task NFR_SEC_001_sign_out_from_the_layout_returns_to_sign_in()
    {
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
        await SignInAsync(client);
        var html = await client.GetStringAsync("/");
        var token = TokenPattern().Match(html).Groups[1].Value;

        var response = await client.PostAsync("/Account/Logout", new FormUrlEncodedContent(
            [new KeyValuePair<string, string>("__RequestVerificationToken", token)]));

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Equal("/Account/Login", response.Headers.Location?.OriginalString);
    }

    private static async Task SignInAsync(HttpClient client)
    {
        var loginPage = await client.GetStringAsync("/Account/Login");
        var token = TokenPattern().Match(loginPage).Groups[1].Value;

        await client.PostAsync("/Account/Login", new FormUrlEncodedContent(
        [
            new KeyValuePair<string, string>("Username", Username),
            new KeyValuePair<string, string>("Password", Password),
            new KeyValuePair<string, string>("__RequestVerificationToken", token)
        ]));
    }

    [GeneratedRegex("name=\"__RequestVerificationToken\" type=\"hidden\" value=\"([^\"]+)\"")]
    private static partial Regex TokenPattern();
}
