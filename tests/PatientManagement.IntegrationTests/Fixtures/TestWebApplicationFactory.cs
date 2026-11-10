using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace PatientManagement.IntegrationTests.Fixtures;

// Web host for tests that use the database. It points the application's connection string at the test database,
// so no test can reach the development database (plan §3A).
public sealed class TestWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        var connectionString = TestDatabaseFixture.ReadConnectionString();
        TestDatabaseGuard.EnsureTestDatabase(connectionString);

        builder.UseSetting("ConnectionStrings:PatientManagement", connectionString);
    }
}
