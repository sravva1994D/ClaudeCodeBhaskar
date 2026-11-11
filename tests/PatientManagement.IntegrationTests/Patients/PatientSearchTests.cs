using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using PatientManagement.Data.Entities;
using PatientManagement.IntegrationTests.Fixtures;

namespace PatientManagement.IntegrationTests.Patients;

// Quick search and the search page, through the web app against the test database. Synthetic data only.
// Test IDs follow CLAUDE.md §4.2 and §10.
[Collection(TestDatabaseCollection.Name)]
public class PatientSearchTests(TestDatabaseFixture database, TestWebApplicationFactory factory)
    : IClassFixture<TestWebApplicationFactory>, IAsyncLifetime
{
    private const string PatientName = "Test Anna Example";
    private const string PatientPhone = "5550101";

    private string patientCode = string.Empty;

    public async Task InitializeAsync()
    {
        await database.ClearTablesAsync("PrescriptionItems", "Prescriptions", "VitalSigns", "Consultations", "Appointments", "Patients");
        await TestSignIn.SeedAccountAsync(database);

        await using var context = database.CreateContext();
        var patient = new Patient
        {
            FullName = PatientName,
            DateOfBirth = new DateOnly(1990, 5, 1),
            Gender = "Female",
            Phone = PatientPhone,
            Email = "anna@example.test",
            Address = "Test address"
        };
        context.Patients.Add(patient);
        await context.SaveChangesAsync();
        patientCode = patient.PatientCode;
    }

    public async Task DisposeAsync()
    {
        await database.ClearTablesAsync("PrescriptionItems", "Prescriptions", "VitalSigns", "Consultations", "Appointments", "Patients");
        await database.ClearTablesAsync("Users");
    }

    [Fact]
    public async Task FR_SRC_001_quick_search_from_the_top_bar_finds_a_patient_by_name()
    {
        using var client = NewClient();
        await TestSignIn.SignInAsync(client);

        var html = await client.GetStringAsync("/Patients/Search?q=anna");

        Assert.Contains(PatientName, html);
        Assert.Contains(patientCode, html);
    }

    [Fact]
    public async Task FR_PAT_004_phone_search_finds_a_patient()
    {
        using var client = NewClient();
        await TestSignIn.SignInAsync(client);

        var html = await client.GetStringAsync("/Patients/Search?q=0101");

        Assert.Contains(PatientName, html);
    }

    [Fact]
    public async Task FR_PAT_004_search_with_no_match_shows_the_no_match_message()
    {
        using var client = NewClient();
        await TestSignIn.SignInAsync(client);

        var html = await client.GetStringAsync("/Patients/Search?q=zara");

        Assert.Contains("No patients match your search.", html);
        Assert.DoesNotContain(PatientName, html);
    }

    [Fact]
    public async Task FR_PAT_004_empty_search_shows_the_error_and_no_results()
    {
        using var client = NewClient();
        await TestSignIn.SignInAsync(client);

        var html = await client.GetStringAsync("/Patients/Search?q=%20%20");

        Assert.Contains("Enter a name or phone number to search.", html);
        Assert.DoesNotContain(PatientName, html);
    }

    [Fact]
    public async Task FR_PAT_004_search_submitted_with_an_empty_box_shows_the_error()
    {
        using var client = NewClient();
        await TestSignIn.SignInAsync(client);

        var html = await client.GetStringAsync("/Patients/Search?q=");

        Assert.Contains("Enter a name or phone number to search.", html);
    }

    [Fact]
    public async Task FR_PAT_004_search_page_opened_without_a_term_shows_no_error()
    {
        using var client = NewClient();
        await TestSignIn.SignInAsync(client);

        var html = await client.GetStringAsync("/Patients/Search");

        Assert.DoesNotContain("Enter a name or phone number to search.", html);
        Assert.DoesNotContain("No patients match your search.", html);
    }

    [Fact]
    public async Task NFR_SEC_001_search_requires_sign_in()
    {
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

        var response = await client.GetAsync("/Patients/Search?q=anna");

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.NotNull(response.Headers.Location);
        var location = new Uri(new Uri("http://localhost"), response.Headers.Location);
        Assert.Equal("/Account/Login", location.AbsolutePath);
    }

    [Fact]
    public async Task FR_SRC_001_quick_search_box_is_shown_when_signed_in()
    {
        using var client = NewClient();
        await TestSignIn.SignInAsync(client);

        var html = await client.GetStringAsync("/");

        Assert.Contains("action=\"/Patients/Search\"", html);
        Assert.Contains("aria-label=\"Quick search\"", html);
    }

    private HttpClient NewClient() =>
        factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
}
