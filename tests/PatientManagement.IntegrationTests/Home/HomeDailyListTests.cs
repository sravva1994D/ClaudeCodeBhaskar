using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;
using PatientManagement.Data.Entities;
using PatientManagement.IntegrationTests.Fixtures;

namespace PatientManagement.IntegrationTests.Home;

// The home page (SCR-03) and the top bar, through the web app against the test database. Synthetic data only.
// Test IDs follow CLAUDE.md §4.2 and §10.
[Collection(TestDatabaseCollection.Name)]
public class HomeDailyListTests(TestDatabaseFixture database, TestWebApplicationFactory factory)
    : IClassFixture<TestWebApplicationFactory>, IAsyncLifetime
{
    public async Task InitializeAsync()
    {
        await database.ClearTablesAsync("Appointments", "Patients");
        await TestSignIn.SeedAccountAsync(database);
    }

    public async Task DisposeAsync()
    {
        await database.ClearTablesAsync("Appointments", "Patients");
        await database.ClearTablesAsync("Users");
    }

    [Fact]
    public async Task FR_APT_002_home_shows_todays_appointments()
    {
        var anna = await AddPatientAsync("Test Anna Example", "5550101");
        await AddAppointmentAsync(anna, DateTime.Today.AddHours(9), "Scheduled");
        using var client = await SignedInClientAsync();

        var html = await client.GetStringAsync("/");

        Assert.Contains("Test Anna Example", html);
        Assert.Contains("09:00", html);
    }

    [Fact]
    public async Task FR_APT_002_home_does_not_show_other_days()
    {
        var ben = await AddPatientAsync("Test Ben Example", "5550102");
        await AddAppointmentAsync(ben, DateTime.Today.AddDays(1).AddHours(10), "Scheduled");
        using var client = await SignedInClientAsync();

        var html = await client.GetStringAsync("/");

        Assert.DoesNotContain("Test Ben Example", html);
        Assert.Contains("No appointments today.", html);
    }

    [Fact]
    public async Task FR_APT_002_home_does_not_show_archived_appointments()
    {
        var anna = await AddPatientAsync("Test Anna Example", "5550101");
        await AddAppointmentAsync(anna, DateTime.Today.AddHours(9), "Completed", archived: true);
        using var client = await SignedInClientAsync();

        var html = await client.GetStringAsync("/");

        Assert.DoesNotContain("Test Anna Example", html);
    }

    [Fact]
    public async Task FR_APT_002_home_offers_new_patient_and_the_full_day_list()
    {
        using var client = await SignedInClientAsync();

        var html = await client.GetStringAsync("/");

        Assert.Contains("href=\"/Patients/Create\"", html);
        Assert.Contains("/Appointments/Daily?date=", html);
    }

    [Fact]
    public async Task FR_SRC_001_home_has_the_quick_search_in_the_top_bar()
    {
        using var client = await SignedInClientAsync();

        var html = await client.GetStringAsync("/");

        Assert.Contains("action=\"/Patients/Search\"", html);
        Assert.Contains("name=\"q\"", html);
    }

    [Fact]
    public async Task FR_APT_002_top_bar_links_to_appointments_when_signed_in()
    {
        using var client = await SignedInClientAsync();

        var html = await client.GetStringAsync("/");

        Assert.Contains("href=\"/Appointments/Daily\"", html);
    }

    [Fact]
    public async Task FR_APT_002_top_bar_hides_appointments_when_signed_out()
    {
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = true });

        var html = await client.GetStringAsync("/Account/Login");

        Assert.DoesNotContain("href=\"/Appointments/Daily\"", html);
    }

    private async Task<int> AddPatientAsync(string fullName, string phone)
    {
        await using var context = database.CreateContext();
        var patient = new Patient
        {
            FullName = fullName,
            DateOfBirth = new DateOnly(1990, 5, 1),
            Gender = "Female",
            Phone = phone,
            Email = "patient@example.test",
            Address = "Test address"
        };
        context.Patients.Add(patient);
        await context.SaveChangesAsync();
        return patient.Id;
    }

    private async Task AddAppointmentAsync(int patientId, DateTime startTime, string status, bool archived = false)
    {
        await using var context = database.CreateContext();
        context.Appointments.Add(new Appointment
        {
            PatientId = patientId,
            StartTime = startTime,
            DurationMinutes = 30,
            Status = status,
            IsArchived = archived
        });
        await context.SaveChangesAsync();
    }

    private async Task<HttpClient> SignedInClientAsync()
    {
        var client = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
        await TestSignIn.SignInAsync(client);
        return client;
    }
}
