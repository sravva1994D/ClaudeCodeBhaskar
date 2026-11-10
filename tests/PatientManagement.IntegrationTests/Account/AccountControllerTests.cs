using System.Net;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using PatientManagement.Data.Entities;
using PatientManagement.IntegrationTests.Fixtures;
using PatientManagement.Web.Security;

namespace PatientManagement.IntegrationTests.Account;

// Sign-in, forgot-password and sign-out through the web app, against the test database.
// Synthetic account and passwords only. Test IDs follow CLAUDE.md §4.2 and §10.
[Collection(TestDatabaseCollection.Name)]
public partial class AccountControllerTests(TestDatabaseFixture database, TestWebApplicationFactory factory)
    : IClassFixture<TestWebApplicationFactory>, IAsyncLifetime
{
    private const string Username = "test.user";
    private const string Email = "test.user@example.test";
    private const string Password = "Abcdef1!";
    private const string GenericLoginError = "The username or password is not correct.";
    private const string ResetMessage = "If the email matches an account, the password has been reset.";

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
    public async Task NFR_SEC_001_login_post_without_anti_forgery_token_is_rejected()
    {
        using var client = NewClient();

        var response = await client.PostAsync("/Account/Login", Form(
            ("Username", Username), ("Password", Password)));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task NFR_SEC_001_correct_login_signs_in_and_opens_the_home_page()
    {
        using var client = NewClient();
        var token = await GetTokenAsync(client, "/Account/Login");

        var response = await client.PostAsync("/Account/Login", Form(
            ("Username", Username), ("Password", Password), ("__RequestVerificationToken", token)));

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Equal("/", response.Headers.Location?.OriginalString);

        var home = await client.GetAsync("/");
        Assert.Equal(HttpStatusCode.OK, home.StatusCode);
    }

    [Fact]
    public async Task NFR_SEC_001_wrong_password_shows_the_generic_error_and_does_not_sign_in()
    {
        using var client = NewClient();
        var token = await GetTokenAsync(client, "/Account/Login");

        var response = await client.PostAsync("/Account/Login", Form(
            ("Username", Username), ("Password", "Wrong-Pass1"), ("__RequestVerificationToken", token)));

        var html = await response.Content.ReadAsStringAsync();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains(GenericLoginError, html);
        Assert.Equal(HttpStatusCode.Redirect, (await client.GetAsync("/")).StatusCode);
    }

    [Fact]
    public async Task NFR_SEC_001_unknown_username_shows_the_same_error_as_a_wrong_password()
    {
        using var client = NewClient();
        var token = await GetTokenAsync(client, "/Account/Login");

        var response = await client.PostAsync("/Account/Login", Form(
            ("Username", "unknown.user"), ("Password", Password), ("__RequestVerificationToken", token)));

        var html = await response.Content.ReadAsStringAsync();
        Assert.Contains(GenericLoginError, html);
    }

    [Fact]
    public async Task NFR_SEC_001_return_url_to_another_site_is_ignored()
    {
        using var client = NewClient();
        var token = await GetTokenAsync(client, "/Account/Login");

        var response = await client.PostAsync("/Account/Login", Form(
            ("Username", Username), ("Password", Password), ("ReturnUrl", "https://example.test/evil"),
            ("__RequestVerificationToken", token)));

        Assert.Equal("/", response.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task NFR_SEC_001_local_return_url_is_followed_after_sign_in()
    {
        using var client = NewClient();
        var token = await GetTokenAsync(client, "/Account/Login");

        var response = await client.PostAsync("/Account/Login", Form(
            ("Username", Username), ("Password", Password), ("ReturnUrl", "/?from=login"),
            ("__RequestVerificationToken", token)));

        Assert.Equal("/?from=login", response.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task NFR_SEC_001_forgot_password_with_no_email_match_shows_the_pdr_03_message()
    {
        using var client = NewClient();
        var token = await GetTokenAsync(client, "/Account/ForgotPassword");

        var response = await client.PostAsync("/Account/ForgotPassword", Form(
            ("Email", "nobody@example.test"), ("NewPassword", "Newpass1!"), ("ConfirmPassword", "Newpass1!"),
            ("__RequestVerificationToken", token)));

        var html = await response.Content.ReadAsStringAsync();
        Assert.Contains(ResetMessage, html);
    }

    [Fact]
    public async Task NFR_SEC_001_forgot_password_with_a_match_resets_the_password_and_signs_in()
    {
        using var client = NewClient();
        var token = await GetTokenAsync(client, "/Account/ForgotPassword");

        var response = await client.PostAsync("/Account/ForgotPassword", Form(
            ("Email", Email), ("NewPassword", "Newpass1!"), ("ConfirmPassword", "Newpass1!"),
            ("__RequestVerificationToken", token)));

        Assert.Equal("/", response.Headers.Location?.OriginalString);
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/")).StatusCode);

        await using var context = database.CreateContext();
        var user = await context.Users.SingleAsync(u => u.Username == Username);
        Assert.True(new AspNetPasswordHasher().Verify(user.PasswordHash, "Newpass1!"));
        Assert.False(new AspNetPasswordHasher().Verify(user.PasswordHash, Password));
    }

    [Fact]
    public async Task NFR_SEC_001_forgot_password_with_mismatched_confirmation_does_not_change_the_password()
    {
        using var client = NewClient();
        var token = await GetTokenAsync(client, "/Account/ForgotPassword");

        var response = await client.PostAsync("/Account/ForgotPassword", Form(
            ("Email", Email), ("NewPassword", "Newpass1!"), ("ConfirmPassword", "Different1!"),
            ("__RequestVerificationToken", token)));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        await using var context = database.CreateContext();
        var user = await context.Users.SingleAsync(u => u.Username == Username);
        Assert.True(new AspNetPasswordHasher().Verify(user.PasswordHash, Password));
    }

    [Fact]
    public async Task NFR_SEC_001_logout_signs_the_user_out()
    {
        using var client = NewClient();
        var token = await GetTokenAsync(client, "/Account/Login");
        await client.PostAsync("/Account/Login", Form(
            ("Username", Username), ("Password", Password), ("__RequestVerificationToken", token)));

        // Anti-forgery tokens are bound to the signed-in user, so the logout form needs a token issued after sign-in.
        var signedInToken = await GetTokenAsync(client, "/Account/Login");
        var response = await client.PostAsync("/Account/Logout", Form(("__RequestVerificationToken", signedInToken)));

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Equal("/Account/Login", response.Headers.Location?.OriginalString);
        Assert.Equal(HttpStatusCode.Redirect, (await client.GetAsync("/")).StatusCode);
    }

    private HttpClient NewClient() =>
        factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

    private static async Task<string> GetTokenAsync(HttpClient client, string path)
    {
        var html = await client.GetStringAsync(path);
        var match = TokenPattern().Match(html);
        Assert.True(match.Success, $"No anti-forgery token found on {path}.");
        return match.Groups[1].Value;
    }

    private static FormUrlEncodedContent Form(params (string Name, string Value)[] fields) =>
        new(fields.Select(field => new KeyValuePair<string, string>(field.Name, field.Value)));

    [GeneratedRegex("name=\"__RequestVerificationToken\" type=\"hidden\" value=\"([^\"]+)\"")]
    private static partial Regex TokenPattern();
}
