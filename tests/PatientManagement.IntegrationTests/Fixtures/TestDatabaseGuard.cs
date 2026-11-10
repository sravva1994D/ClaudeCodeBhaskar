using Microsoft.Data.SqlClient;

namespace PatientManagement.IntegrationTests.Fixtures;

// Infrastructure guard, not a requirement check. It stops the tests from ever running
// against the development database or any other database (plan section 3A).
public static class TestDatabaseGuard
{
    private const string RequiredSuffix = "_Test";

    public static string EnsureTestDatabase(string connectionString)
    {
        var databaseName = new SqlConnectionStringBuilder(connectionString).InitialCatalog;

        if (string.IsNullOrWhiteSpace(databaseName) || !databaseName.EndsWith(RequiredSuffix, StringComparison.Ordinal))
        {
            throw new InvalidOperationException(
                $"Refusing to run integration tests against database '{databaseName}'. " +
                $"The test database name must end in '{RequiredSuffix}'.");
        }

        return databaseName;
    }
}
