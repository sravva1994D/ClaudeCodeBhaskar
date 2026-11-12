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
        await database.ClearTablesAsync("PrescriptionItems", "Prescriptions", "VitalSigns", "Consultations", "Appointments", "Patients");
        await TestSignIn.SeedAccountAsync(database);
    }

    public async Task DisposeAsync()
    {
        await database.ClearTablesAsync("PrescriptionItems", "Prescriptions", "VitalSigns", "Consultations", "Appointments", "Patients");
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

    [Fact]
    public async Task FR_SRC_002_home_lists_the_five_latest_visited_patients_in_visit_order()
    {
        for (var index = 1; index <= 6; index++)
        {
            var patient = await AddPatientAsync($"Test Recent {index}", $"55502{index:D2}");
            await AddVisitAsync(patient, DateTime.Today.AddDays(index - 10).AddHours(9));
        }
        using var client = await SignedInClientAsync();

        var html = await client.GetStringAsync("/");

        Assert.Contains("Test Recent 6", html);
        Assert.Contains("Test Recent 2", html);
        Assert.DoesNotContain("Test Recent 1<", html);
        Assert.True(html.IndexOf("Test Recent 6", StringComparison.Ordinal) < html.IndexOf("Test Recent 2", StringComparison.Ordinal));
    }

    [Fact]
    public async Task FR_SRC_002_home_leaves_out_patients_with_no_visit()
    {
        await AddPatientAsync("Test Never Visited", "5550301");
        var visited = await AddPatientAsync("Test Visited Once", "5550302");
        await AddVisitAsync(visited, DateTime.Today.AddHours(9));
        using var client = await SignedInClientAsync();

        var html = await client.GetStringAsync("/");

        Assert.Contains("Test Visited Once", html);
        Assert.DoesNotContain("Test Never Visited", html);
    }

    [Fact]
    public async Task FR_SRC_002_home_says_so_when_no_patient_has_a_visit()
    {
        await AddPatientAsync("Test Never Visited", "5550301");
        using var client = await SignedInClientAsync();

        var html = await client.GetStringAsync("/");

        Assert.Contains("No recent patients yet.", html);
    }

    [Fact]
    public async Task FR_SRC_002_recent_patient_name_links_to_the_profile()
    {
        var visited = await AddPatientAsync("Test Visited Once", "5550302");
        await AddVisitAsync(visited, DateTime.Today.AddHours(9));
        using var client = await SignedInClientAsync();

        var html = await client.GetStringAsync("/");

        Assert.Contains($"href=\"/Patients/Details/{visited}\"", html);
    }

    private async Task AddVisitAsync(int patientId, DateTime visitTime)
    {
        await using var context = database.CreateContext();
        context.Consultations.Add(new Consultation { PatientId = patientId, VisitDateTime = visitTime });
        await context.SaveChangesAsync();
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
