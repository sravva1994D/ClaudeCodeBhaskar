using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;
using PatientManagement.Data.Entities;
using PatientManagement.IntegrationTests.Fixtures;

namespace PatientManagement.IntegrationTests.Visits;

// Visit history (SCR-07), visit detail (SCR-08), the profile's last visit and the links between them,
// through the web app against the test database. Synthetic data only. Test IDs follow CLAUDE.md §4.2 and §10.
[Collection(TestDatabaseCollection.Name)]
public class VisitPagesTests(TestDatabaseFixture database, TestWebApplicationFactory factory)
    : IClassFixture<TestWebApplicationFactory>, IAsyncLifetime
{
    private static readonly DateTime Earlier = new(2026, 10, 2, 9, 0, 0);
    private static readonly DateTime Later = new(2026, 11, 3, 14, 30, 0);

    private int patientId;
    private int otherPatientId;

    public async Task InitializeAsync()
    {
        await Clear();
        await TestSignIn.SeedAccountAsync(database);

        await using var context = database.CreateContext();
        var anna = NewPatient("Test Anna Example", "5550101");
        var ben = NewPatient("Test Ben Example", "5550102");
        context.Patients.AddRange(anna, ben);
        await context.SaveChangesAsync();
        patientId = anna.Id;
        otherPatientId = ben.Id;
    }

    public async Task DisposeAsync()
    {
        await Clear();
        await database.ClearTablesAsync("Users");
    }

    [Fact]
    public async Task FR_HIS_001_visit_history_lists_the_patients_visits_newest_first()
    {
        var older = await SeedVisitAsync(patientId, Earlier, complaints: "Test older complaint");
        var newer = await SeedVisitAsync(patientId, Later, complaints: "Test newer complaint");
        await SeedVisitAsync(otherPatientId, Later.AddDays(1), complaints: "Test other patient complaint");
        using var client = await SignedInClientAsync();

        var html = await client.GetStringAsync($"/Patients/{patientId}/Visits");

        Assert.Contains($"/Visits/Details/{newer}", html);
        Assert.Contains($"/Visits/Details/{older}", html);
        Assert.True(html.IndexOf($"/Visits/Details/{newer}") < html.IndexOf($"/Visits/Details/{older}"));
        Assert.DoesNotContain("Test other patient", html);
    }

    [Fact]
    public async Task FR_HIS_001_an_in_progress_visit_is_listed_with_its_status()
    {
        await SeedVisitAsync(patientId, Later);
        using var client = await SignedInClientAsync();

        var html = await client.GetStringAsync($"/Patients/{patientId}/Visits");

        Assert.Contains("In progress", html);
    }

    [Fact]
    public async Task FR_HIS_002_visit_detail_shows_the_vitals_complaints_and_diagnosis()
    {
        var visitId = await SeedVisitAsync(patientId, Later, complaints: "Test cough", diagnosis: "Test diagnosis", withVitals: true);
        using var client = await SignedInClientAsync();

        var html = await client.GetStringAsync($"/Visits/Details/{visitId}");

        Assert.Contains("120/80 mmHg", html);
        Assert.Contains("72 bpm", html);
        Assert.Contains("Test cough", html);
        Assert.Contains("Test diagnosis", html);
    }

    [Fact]
    public async Task FR_CMP_001_complaints_are_shown_in_the_visit_detail()
    {
        var visitId = await SeedVisitAsync(patientId, Later, complaints: "Test headache");
        using var client = await SignedInClientAsync();

        var html = await client.GetStringAsync($"/Visits/Details/{visitId}");

        Assert.Contains("Test headache", html);
    }

    [Fact]
    public async Task FR_DIA_001_diagnosis_is_shown_in_the_visit_detail()
    {
        var visitId = await SeedVisitAsync(patientId, Later, diagnosis: "Test viral fever");
        using var client = await SignedInClientAsync();

        var html = await client.GetStringAsync($"/Visits/Details/{visitId}");

        Assert.Contains("Test viral fever", html);
    }

    [Fact]
    public async Task FR_HIS_002_detail_without_vitals_says_so()
    {
        var visitId = await SeedVisitAsync(patientId, Later);
        using var client = await SignedInClientAsync();

        var html = await client.GetStringAsync($"/Visits/Details/{visitId}");

        Assert.Contains("No vitals recorded", html);
    }

    [Fact]
    public async Task FR_SRC_003_profile_links_to_visit_history_and_the_history_links_back()
    {
        var visitId = await SeedVisitAsync(patientId, Later);
        using var client = await SignedInClientAsync();

        var profile = await client.GetStringAsync($"/Patients/Details/{patientId}");
        var history = await client.GetStringAsync($"/Patients/{patientId}/Visits");
        var detail = await client.GetStringAsync($"/Visits/Details/{visitId}");

        Assert.Contains($"/Patients/{patientId}/Visits", profile);
        Assert.Contains($"/Patients/Details/{patientId}", history);
        Assert.Contains($"/Patients/{patientId}/Visits", detail);
    }

    [Fact]
    public async Task FR_HIS_001_profile_shows_the_latest_visit_time()
    {
        await SeedVisitAsync(patientId, Earlier);
        await SeedVisitAsync(patientId, Later);
        using var client = await SignedInClientAsync();

        var html = await client.GetStringAsync($"/Patients/Details/{patientId}");

        Assert.Contains("2026-11-03 14:30", html);
    }

    [Fact]
    public async Task FR_HIS_001_profile_with_no_visit_says_no_visits_yet()
    {
        using var client = await SignedInClientAsync();

        var html = await client.GetStringAsync($"/Patients/Details/{patientId}");

        Assert.Contains("No visits yet", html);
    }

    [Fact]
    public async Task FR_HIS_001_visit_history_of_an_unknown_patient_is_not_found()
    {
        using var client = await SignedInClientAsync();

        var response = await client.GetAsync("/Patients/999999/Visits");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task FR_HIS_002_unknown_visit_detail_is_not_found()
    {
        using var client = await SignedInClientAsync();

        var response = await client.GetAsync("/Visits/Details/999999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task FR_HIS_001_visit_history_needs_sign_in()
    {
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

        var response = await client.GetAsync($"/Patients/{patientId}/Visits");

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Equal("/Account/Login", new Uri(new Uri("http://localhost"), response.Headers.Location!).AbsolutePath);
    }

    private static Patient NewPatient(string fullName, string phone) => new()
    {
        FullName = fullName,
        DateOfBirth = new DateOnly(1990, 5, 1),
        Gender = "Female",
        Phone = phone,
        Email = "patient@example.test",
        Address = "Test address"
    };

    private async Task<int> SeedVisitAsync(
        int patient, DateTime visitTime, string? complaints = null, string? diagnosis = null, bool withVitals = false)
    {
        await using var context = database.CreateContext();
        var visit = new Consultation
        {
            PatientId = patient,
            VisitDateTime = visitTime,
            Complaints = complaints,
            Diagnosis = diagnosis
        };
        if (withVitals)
        {
            visit.Vitals = new VitalSign
            {
                TemperatureC = 36.8m,
                SystolicMmHg = 120,
                DiastolicMmHg = 80,
                PulseBpm = 72
            };
        }

        context.Consultations.Add(visit);
        await context.SaveChangesAsync();
        return visit.Id;
    }

    private async Task Clear() =>
        await database.ClearTablesAsync("PrescriptionItems", "Prescriptions", "VitalSigns", "Consultations", "Appointments", "Patients");

    private async Task<HttpClient> SignedInClientAsync()
    {
        var client = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
        await TestSignIn.SignInAsync(client);
        return client;
    }
}
