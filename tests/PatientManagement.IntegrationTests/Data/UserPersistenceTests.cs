using Microsoft.EntityFrameworkCore;
using PatientManagement.Data.Entities;
using PatientManagement.IntegrationTests.Fixtures;

namespace PatientManagement.IntegrationTests.Data;

// Data tests for the Users table against SQL Server (no in-memory provider; Documentation/02-Planning/14-Testing-Strategy.md §1).
// They use synthetic values only. Test IDs follow CLAUDE.md §4.2 and §10.
[Collection(TestDatabaseCollection.Name)]
public class UserPersistenceTests(TestDatabaseFixture fixture) : IAsyncLifetime
{
    public async Task InitializeAsync() => await fixture.ClearTablesAsync("Users");

    public async Task DisposeAsync() => await fixture.ClearTablesAsync("Users");

    [Fact]
    public async Task NFR_SEC_001_user_is_saved_and_found_by_username()
    {
        await using (var writer = fixture.CreateContext())
        {
            writer.Users.Add(new User
            {
                Username = "test.user",
                PasswordHash = "hash-value-for-test",
                Email = "test.user@example.test"
            });
            await writer.SaveChangesAsync();
        }

        await using var reader = fixture.CreateContext();
        var user = await reader.Users.SingleAsync(u => u.Username == "test.user");

        Assert.Equal("hash-value-for-test", user.PasswordHash);
        Assert.Equal("test.user@example.test", user.Email);
    }

    [Fact]
    public async Task NFR_SEC_001_username_is_unique()
    {
        await using (var first = fixture.CreateContext())
        {
            first.Users.Add(new User { Username = "test.user", PasswordHash = "hash-one", Email = "one@example.test" });
            await first.SaveChangesAsync();
        }

        await using var second = fixture.CreateContext();
        second.Users.Add(new User { Username = "test.user", PasswordHash = "hash-two", Email = "two@example.test" });

        await Assert.ThrowsAsync<DbUpdateException>(() => second.SaveChangesAsync());
    }

    [Fact]
    public async Task NFR_SEC_001_password_hash_column_is_required()
    {
        await using var context = fixture.CreateContext();
        context.Users.Add(new User { Username = "test.user", PasswordHash = null!, Email = "test.user@example.test" });

        await Assert.ThrowsAsync<DbUpdateException>(() => context.SaveChangesAsync());
    }
}
