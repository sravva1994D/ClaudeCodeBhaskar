using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using PatientManagement.Business.Common;
using PatientManagement.Business.DTOs;
using PatientManagement.Business.Interfaces;
using PatientManagement.Data.Entities;
using PatientManagement.IntegrationTests.Fixtures;
using PatientManagement.Web.Security;
using PatientManagement.Web.Setup;

namespace PatientManagement.IntegrationTests.Setup;

// Tests for the seed command (DEC-018). Synthetic values only. Test IDs follow CLAUDE.md §4.2 and §10.
[Collection(TestDatabaseCollection.Name)]
public partial class SeedUserRunnerTests(TestDatabaseFixture database, TestWebApplicationFactory factory)
    : IClassFixture<TestWebApplicationFactory>, IAsyncLifetime
{
    private const string Username = "test.user";
    private const string Email = "test.user@example.test";
    private const string Password = "Abcdef1!";

    public async Task InitializeAsync() => await database.ClearTablesAsync("Users");

    public async Task DisposeAsync() => await database.ClearTablesAsync("Users");

    [Fact]
    public async Task NFR_SEC_001_seed_passes_the_typed_values_to_the_service()
    {
        var auth = Substitute.For<IAuthService>();
        auth.CreateInitialAccountAsync(Username, Email, Password, Arg.Any<CancellationToken>())
            .Returns(Result<SignedInUser>.Ok(new SignedInUser(1, Username)));
        var console = new FakeConsole(Username, Email, Password, Password);

        var exitCode = await new SeedUserRunner(auth, console).RunAsync();

        Assert.Equal(0, exitCode);
        await auth.Received(1).CreateInitialAccountAsync(Username, Email, Password, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task NFR_SEC_001_seed_stops_when_the_passwords_do_not_match()
    {
        var auth = Substitute.For<IAuthService>();
        var console = new FakeConsole(Username, Email, Password, "Different1!");

        var exitCode = await new SeedUserRunner(auth, console).RunAsync();

        Assert.Equal(1, exitCode);
        await auth.DidNotReceiveWithAnyArgs().CreateInitialAccountAsync(default!, default!, default!);
    }

    [Fact]
    public async Task NFR_SEC_001_seed_reports_the_service_errors_and_fails()
    {
        var auth = Substitute.For<IAuthService>();
        auth.CreateInitialAccountAsync(default!, default!, default!, default)
            .ReturnsForAnyArgs(Result<SignedInUser>.Fail(
                new ResultMessage("Password.NoSymbol", "The password must include a symbol.", "Password")));
        var console = new FakeConsole(Username, Email, "abcdefg1", "abcdefg1");

        var exitCode = await new SeedUserRunner(auth, console).RunAsync();

        Assert.Equal(1, exitCode);
        Assert.Contains("The password must include a symbol.", console.Output);
    }

    [Fact]
    public async Task NFR_SEC_001_seed_command_output_never_contains_the_password()
    {
        var auth = Substitute.For<IAuthService>();
        auth.CreateInitialAccountAsync(default!, default!, default!, default)
            .ReturnsForAnyArgs(Result<SignedInUser>.Ok(new SignedInUser(1, Username)));
        var console = new FakeConsole(Username, Email, Password, Password);

        await new SeedUserRunner(auth, console).RunAsync();

        Assert.All(console.Output, line => Assert.DoesNotContain(Password, line, StringComparison.Ordinal));
    }

    [Fact]
    public async Task NFR_SEC_001_seed_creates_one_account_and_stores_only_a_hash()
    {
        await database.ClearTablesAsync("Users");
        using var scope = factory.Services.CreateScope();
        var console = new FakeConsole(Username, Email, Password, Password);
        var runner = new SeedUserRunner(scope.ServiceProvider.GetRequiredService<IAuthService>(), console);

        var exitCode = await runner.RunAsync();

        Assert.Equal(0, exitCode);
        await using var context = database.CreateContext();
        var user = await context.Users.SingleAsync();
        Assert.NotEqual(Password, user.PasswordHash);
        Assert.DoesNotContain(Password, user.PasswordHash, StringComparison.Ordinal);
        Assert.True(new AspNetPasswordHasher().Verify(user.PasswordHash, Password));
    }

    [Fact]
    public async Task NFR_SEC_001_second_seed_run_is_refused_and_changes_nothing()
    {
        await database.ClearTablesAsync("Users");
        using var scope = factory.Services.CreateScope();
        var auth = scope.ServiceProvider.GetRequiredService<IAuthService>();
        await new SeedUserRunner(auth, new FakeConsole(Username, Email, Password, Password)).RunAsync();

        var secondConsole = new FakeConsole("other.user", "other.user@example.test", "Other-Pass1", "Other-Pass1");
        var exitCode = await new SeedUserRunner(auth, secondConsole).RunAsync();

        Assert.Equal(1, exitCode);
        await using var context = database.CreateContext();
        var users = await context.Users.ToListAsync();
        Assert.Single(users);
        Assert.Equal(Username, users[0].Username);
    }

    [Fact]
    public void NFR_SEC_001_no_password_is_set_in_the_web_configuration_files()
    {
        var webProject = Path.Combine(RepositoryRoot(), "src", "PatientManagement.Web");
        var configFiles = Directory.GetFiles(webProject, "appsettings*.json");

        Assert.NotEmpty(configFiles);
        foreach (var file in configFiles)
        {
            Assert.DoesNotContain("password", File.ReadAllText(file), StringComparison.OrdinalIgnoreCase);
        }
    }

    [Fact]
    public void NFR_SEC_001_no_password_value_is_written_in_web_source_code()
    {
        var webProject = Path.Combine(RepositoryRoot(), "src", "PatientManagement.Web");
        var sources = Directory.GetFiles(webProject, "*.cs", SearchOption.AllDirectories)
            .Where(file => !file.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}")
                        && !file.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}"));

        foreach (var file in sources)
        {
            Assert.False(
                PasswordLiteral().IsMatch(File.ReadAllText(file)),
                $"A password value is written in source: {Path.GetFileName(file)}");
        }
    }

    private static string RepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "PatientManagement.sln")))
        {
            directory = directory.Parent;
        }

        return directory?.FullName ?? throw new InvalidOperationException("Repository root not found.");
    }

    // A password assigned a quoted literal, for example: Password = "abc". Prompt labels such as "Password: " are not values.
    [GeneratedRegex("(?i)password\\w*\\s*=\\s*\"[^\"]+\"")]
    private static partial Regex PasswordLiteral();

    private sealed class FakeConsole(string username, string email, string password, string confirmation) : IConsoleIO
    {
        private readonly Queue<string> lines = new([username, email]);
        private readonly Queue<string> secrets = new([password, confirmation]);

        public List<string> Output { get; } = [];

        public string? ReadLine(string prompt) => lines.Dequeue();

        public string ReadSecret(string prompt) => secrets.Dequeue();

        public void WriteLine(string text) => Output.Add(text);
    }
}
