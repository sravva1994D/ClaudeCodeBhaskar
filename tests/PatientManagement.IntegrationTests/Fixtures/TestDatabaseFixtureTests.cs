using Microsoft.EntityFrameworkCore;

namespace PatientManagement.IntegrationTests.Fixtures;

// Infrastructure tests. They verify the test database setup and the name guard. They name no requirement ID.
public class TestDatabaseGuardTests
{
    [Fact]
    public void Guard_refuses_the_development_database_name()
    {
        var connectionString = "Server=localhost;Database=PatientManagement;Trusted_Connection=True";

        Assert.Throws<InvalidOperationException>(() => TestDatabaseGuard.EnsureTestDatabase(connectionString));
    }

    [Fact]
    public void Guard_refuses_a_name_that_only_contains_test()
    {
        var connectionString = "Server=localhost;Database=PatientManagement_Test_old;Trusted_Connection=True";

        Assert.Throws<InvalidOperationException>(() => TestDatabaseGuard.EnsureTestDatabase(connectionString));
    }

    [Fact]
    public void Guard_refuses_a_connection_string_without_a_database()
    {
        var connectionString = "Server=localhost;Trusted_Connection=True";

        Assert.Throws<InvalidOperationException>(() => TestDatabaseGuard.EnsureTestDatabase(connectionString));
    }

    [Fact]
    public void Guard_accepts_the_test_database_name()
    {
        var connectionString = "Server=localhost;Database=PatientManagement_Test;Trusted_Connection=True";

        var name = TestDatabaseGuard.EnsureTestDatabase(connectionString);

        Assert.Equal("PatientManagement_Test", name);
    }
}

public class TestDatabaseFixtureTests(TestDatabaseFixture fixture) : IClassFixture<TestDatabaseFixture>
{
    [Fact]
    public async Task Test_database_is_reachable_and_is_the_test_database()
    {
        await using var context = fixture.CreateContext();

        Assert.True(await context.Database.CanConnectAsync());

        // Ask the server which database the session is using, not the connection string.
        var databaseName = await context.Database.SqlQueryRaw<string>("SELECT DB_NAME() AS [Value]").SingleAsync();
        Assert.EndsWith("_Test", databaseName, StringComparison.Ordinal);
    }
}
