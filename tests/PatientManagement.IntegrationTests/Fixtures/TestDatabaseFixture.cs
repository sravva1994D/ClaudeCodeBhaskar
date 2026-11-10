using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using PatientManagement.Data.Context;

namespace PatientManagement.IntegrationTests.Fixtures;

// Shared by data and web integration tests. Applies migrations once per test run and
// gives each test a context. No in-memory provider is used (Documentation/02-Planning/14-Testing-Strategy.md section 1).
public sealed partial class TestDatabaseFixture : IAsyncLifetime
{
    public string ConnectionString { get; } = ReadConnectionString();

    public DbContextOptions<PatientManagementDbContext> Options { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        TestDatabaseGuard.EnsureTestDatabase(ConnectionString);

        Options = new DbContextOptionsBuilder<PatientManagementDbContext>()
            .UseSqlServer(ConnectionString)
            .Options;

        await using var context = CreateContext();
        await context.Database.MigrateAsync();
    }

    public Task DisposeAsync() => Task.CompletedTask;

    public PatientManagementDbContext CreateContext() => new(Options);

    // Deletes all rows from the given tables. Pass child tables before parent tables.
    public async Task ClearTablesAsync(params string[] tablesInForeignKeyOrder)
    {
        await using var context = CreateContext();
        foreach (var table in tablesInForeignKeyOrder)
        {
            if (!IdentifierPattern().IsMatch(table))
            {
                throw new ArgumentException($"Invalid table name '{table}'.", nameof(tablesInForeignKeyOrder));
            }

            // The table name is checked against IdentifierPattern above; it cannot be a parameter.
#pragma warning disable EF1002
            await context.Database.ExecuteSqlRawAsync($"DELETE FROM [{table}];");
#pragma warning restore EF1002
        }
    }

    // Runs the action inside a transaction that is always rolled back.
    public async Task RunInRollbackAsync(Func<PatientManagementDbContext, Task> action)
    {
        await using var context = CreateContext();
        await using var transaction = await context.Database.BeginTransactionAsync();
        await action(context);
        await transaction.RollbackAsync();
    }

    [GeneratedRegex("^[A-Za-z_][A-Za-z0-9_]*$")]
    private static partial Regex IdentifierPattern();

    public static string ReadConnectionString()
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false)
            .AddUserSecrets<TestDatabaseFixture>(optional: true)
            .AddEnvironmentVariables()
            .Build();

        return configuration.GetConnectionString("PatientManagementTest")
            ?? throw new InvalidOperationException(
                "Connection string 'PatientManagementTest' is not configured for the integration tests.");
    }
}
