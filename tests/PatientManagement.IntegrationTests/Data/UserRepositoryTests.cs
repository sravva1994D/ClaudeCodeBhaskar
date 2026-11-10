using Microsoft.EntityFrameworkCore;
using PatientManagement.Data.Entities;
using PatientManagement.Data.Repositories;
using PatientManagement.IntegrationTests.Fixtures;

namespace PatientManagement.IntegrationTests.Data;

// Repository tests against SQL Server. Synthetic values only. Test IDs follow CLAUDE.md §4.2 and §10.
[Collection(TestDatabaseCollection.Name)]
public class UserRepositoryTests(TestDatabaseFixture fixture) : IAsyncLifetime
{
    private const string Username = "test.user";
    private const string Email = "test.user@example.test";

    public async Task InitializeAsync()
    {
        await fixture.ClearTablesAsync("Users");
        await using var context = fixture.CreateContext();
        context.Users.Add(new User { Username = Username, PasswordHash = "hash-before", Email = Email });
        await context.SaveChangesAsync();
    }

    public async Task DisposeAsync() => await fixture.ClearTablesAsync("Users");

    [Fact]
    public async Task NFR_SEC_001_finds_the_account_by_username()
    {
        await using var context = fixture.CreateContext();
        var repository = new UserRepository(context);

        var user = await repository.GetByUsernameAsync(Username);

        Assert.NotNull(user);
        Assert.Equal(Email, user.Email);
    }

    [Fact]
    public async Task NFR_SEC_001_returns_null_for_an_unknown_username()
    {
        await using var context = fixture.CreateContext();
        var repository = new UserRepository(context);

        var user = await repository.GetByUsernameAsync("someone.else");

        Assert.Null(user);
    }

    [Fact]
    public async Task NFR_SEC_001_finds_the_account_by_email()
    {
        await using var context = fixture.CreateContext();
        var repository = new UserRepository(context);

        var user = await repository.GetByEmailAsync(Email);

        Assert.NotNull(user);
        Assert.Equal(Username, user.Username);
    }

    [Fact]
    public async Task NFR_SEC_001_returns_null_for_an_unknown_email()
    {
        await using var context = fixture.CreateContext();
        var repository = new UserRepository(context);

        var user = await repository.GetByEmailAsync("nobody@example.test");

        Assert.Null(user);
    }

    [Fact]
    public async Task NFR_SEC_002_update_saves_the_new_password_hash()
    {
        await using var context = fixture.CreateContext();
        var repository = new UserRepository(context);
        var user = await repository.GetByUsernameAsync(Username);
        Assert.NotNull(user);

        user.PasswordHash = "hash-after";
        await repository.UpdateAsync(user);

        await using var reader = fixture.CreateContext();
        var saved = await reader.Users.SingleAsync(u => u.Username == Username);
        Assert.Equal("hash-after", saved.PasswordHash);
    }

    [Fact]
    public async Task NFR_SEC_001_any_is_true_when_an_account_exists()
    {
        await using var context = fixture.CreateContext();
        var repository = new UserRepository(context);

        Assert.True(await repository.AnyAsync());
    }

    [Fact]
    public async Task NFR_SEC_001_any_is_false_when_no_account_exists()
    {
        await fixture.ClearTablesAsync("Users");
        await using var context = fixture.CreateContext();
        var repository = new UserRepository(context);

        Assert.False(await repository.AnyAsync());
    }

    [Fact]
    public async Task NFR_SEC_001_add_saves_the_account()
    {
        await fixture.ClearTablesAsync("Users");
        await using (var context = fixture.CreateContext())
        {
            var repository = new UserRepository(context);
            await repository.AddAsync(new User { Username = Username, PasswordHash = "hash-added", Email = Email });
        }

        await using var reader = fixture.CreateContext();
        var saved = await reader.Users.SingleAsync(u => u.Username == Username);
        Assert.Equal("hash-added", saved.PasswordHash);
    }
}
