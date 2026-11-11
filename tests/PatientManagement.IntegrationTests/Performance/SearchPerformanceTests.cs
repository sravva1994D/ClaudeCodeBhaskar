using System.Diagnostics;
using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;
using PatientManagement.Data.Entities;
using PatientManagement.IntegrationTests.Fixtures;
using Xunit.Abstractions;

namespace PatientManagement.IntegrationTests.Performance;

// Search timing on 1,000 synthetic patients (NFR-PRF-002, SC-002; PDR-30: median of 5 runs).
// The test records the timings and checks that each search returns the right rows. It does not fail on speed:
// the 2 to 5 second targets are guidelines (BRD, Non-Functional Requirements), and a miss is reported, not hidden.
// Timings are measured in-process, through the web stack, on the test database; browser timing is a separate manual run.
[Collection(TestDatabaseCollection.Name)]
public class SearchPerformanceTests(TestDatabaseFixture database, TestWebApplicationFactory factory, ITestOutputHelper output)
    : IClassFixture<TestWebApplicationFactory>, IAsyncLifetime
{
    private const int PatientCount = 1000;
    private const int Runs = 5;

    private static readonly string[] Surnames =
        ["Anderson", "Brown", "Clark", "Davis", "Evans", "Foster", "Green", "Harris", "Ibrahim", "Jones"];

    // Each term with the number of patients it must match in the generated set.
    private static readonly (string Term, int ExpectedMatches)[] Terms =
    [
        ("Patient 0500", 1),
        ("Patient 0001", 1),
        ("Brown", 100),
        ("Test", 1000),
        ("00000042", 1),
        ("00000999", 1),
        ("Harris", 100),
        ("Example", 0),
        ("Clark", 100),
        ("zzzz", 0)
    ];

    public async Task InitializeAsync()
    {
        await database.ClearTablesAsync("Appointments", "Patients");
        await TestSignIn.SeedAccountAsync(database);

        await using var context = database.CreateContext();
        context.Patients.AddRange(Enumerable.Range(1, PatientCount).Select(Synthetic));
        await context.SaveChangesAsync();
    }

    public async Task DisposeAsync()
    {
        await database.ClearTablesAsync("Appointments", "Patients");
        await database.ClearTablesAsync("Users");
    }

    [Fact]
    public async Task NFR_PRF_002_search_returns_the_right_rows_and_its_timings_are_recorded()
    {
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
        await TestSignIn.SignInAsync(client);

        output.WriteLine("Term | Matches | Median ms | Min ms | Max ms | Runs (ms)");

        foreach (var (term, expected) in Terms)
        {
            var url = $"/Patients/Search?q={Uri.EscapeDataString(term)}";

            // One request warms the caches and is not timed. The timed runs follow.
            await client.GetStringAsync(url);

            var timings = new List<double>();
            string? html = null;
            for (var run = 0; run < Runs; run++)
            {
                var watch = Stopwatch.StartNew();
                var response = await client.GetAsync(url);
                html = await response.Content.ReadAsStringAsync();
                watch.Stop();

                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                timings.Add(watch.Elapsed.TotalMilliseconds);
            }

            var matches = CountRows(html!);
            Assert.Equal(expected, matches);

            timings.Sort();
            output.WriteLine(
                $"{term} | {matches} | {timings[Runs / 2]:F0} | {timings[0]:F0} | {timings[^1]:F0} | " +
                string.Join(", ", timings.Select(t => t.ToString("F0"))));
        }
    }

    private static int CountRows(string html) =>
        // A result row has a link to a profile. The "Register" link is not a profile link.
        System.Text.RegularExpressions.Regex.Matches(html, "href=\"/Patients/Details/").Count;

    private static Patient Synthetic(int index) => new()
    {
        FullName = $"Test Patient {index:D4} {Surnames[index % Surnames.Length]}",
        DateOfBirth = new DateOnly(1950, 1, 1).AddDays((index * 37) % 20000),
        Gender = index % 3 == 0 ? "Male" : index % 3 == 1 ? "Female" : "Other",
        Phone = $"55{index:D8}",
        Email = $"test.patient{index}@example.test",
        Address = $"Test address {index}"
    };
}
