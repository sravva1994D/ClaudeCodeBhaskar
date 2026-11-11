using System.Text.RegularExpressions;
using PatientManagement.Data.Entities;
using PatientManagement.Web.Security;

namespace PatientManagement.IntegrationTests.Fixtures;

// Creates the test account and signs a test client in through the sign-in form. Synthetic account only.
public static partial class TestSignIn
{
    public const string Username = "test.user";
    public const string Email = "test.user@example.test";
    public const string Password = "Abcdef1!";

    public static async Task SeedAccountAsync(TestDatabaseFixture database)
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

    public static async Task SignInAsync(HttpClient client)
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

    // Reads the anti-forgery token from a page. Tokens are bound to the signed-in user, so read them after sign-in.
    public static async Task<string> AntiForgeryTokenAsync(HttpClient client, string path)
    {
        var html = await client.GetStringAsync(path);
        return TokenPattern().Match(html).Groups[1].Value;
    }

    [GeneratedRegex("name=\"__RequestVerificationToken\" type=\"hidden\" value=\"([^\"]+)\"")]
    private static partial Regex TokenPattern();
}
