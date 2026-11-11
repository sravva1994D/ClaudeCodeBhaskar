using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using PatientManagement.Business.Services;
using PatientManagement.Data.Entities;
using PatientManagement.IntegrationTests.Fixtures;

namespace PatientManagement.IntegrationTests.Patients;

// Register, edit and profile screens through the web app, against the test database. Synthetic data only.
// Test IDs follow CLAUDE.md §4.2 and §10.
[Collection(TestDatabaseCollection.Name)]
public class PatientPagesTests(TestDatabaseFixture database, TestWebApplicationFactory factory)
    : IClassFixture<TestWebApplicationFactory>, IAsyncLifetime
{
    private const string ExistingName = "Test Anna Example";
    private const string ExistingPhone = "5550101";
    private static readonly DateOnly ExistingBirth = new(1990, 5, 1);

    private int existingId;

    public async Task InitializeAsync()
    {
        await database.ClearTablesAsync("Appointments", "Patients");
        await TestSignIn.SeedAccountAsync(database);

        await using var context = database.CreateContext();
        var patient = new Patient
        {
            FullName = ExistingName,
            DateOfBirth = ExistingBirth,
            Gender = "Female",
            Phone = ExistingPhone,
            Email = "anna@example.test",
            Address = "Test address"
        };
        context.Patients.Add(patient);
        await context.SaveChangesAsync();
        existingId = patient.Id;
    }

    public async Task DisposeAsync()
    {
        await database.ClearTablesAsync("Appointments", "Patients");
        await database.ClearTablesAsync("Users");
    }

    [Fact]
    public async Task FR_PAT_001_register_saves_every_field_and_opens_the_profile()
    {
        using var client = await SignedInClientAsync();
        var token = await TestSignIn.AntiForgeryTokenAsync(client, "/Patients/Create");

        var response = await client.PostAsync("/Patients/Create", Form(
            ("FullName", "Test Ben Example"), ("DateOfBirth", "1985-02-14"), ("Gender", "Male"),
            ("Phone", "+91 98765-43210"), ("Email", "ben@example.test"), ("Address", "Test street 1"),
            ("__RequestVerificationToken", token)));

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        await using var context = database.CreateContext();
        var saved = await context.Patients.SingleAsync(p => p.FullName == "Test Ben Example");
        Assert.Equal(new DateOnly(1985, 2, 14), saved.DateOfBirth);
        Assert.Equal("Male", saved.Gender);
        Assert.Equal("+91 98765-43210", saved.Phone);
        Assert.Equal("ben@example.test", saved.Email);
        Assert.Equal("Test street 1", saved.Address);
        Assert.StartsWith("PT-", saved.PatientCode);
        Assert.Equal($"/Patients/Details/{saved.Id}", response.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task FR_PAT_001_register_with_a_missing_field_shows_the_error_and_saves_nothing()
    {
        using var client = await SignedInClientAsync();
        var token = await TestSignIn.AntiForgeryTokenAsync(client, "/Patients/Create");

        var response = await client.PostAsync("/Patients/Create", Form(
            ("FullName", "Test Ben Example"), ("DateOfBirth", "1985-02-14"), ("Gender", "Male"),
            ("Phone", "5550199"), ("Email", "ben@example.test"), ("Address", ""),
            ("__RequestVerificationToken", token)));

        var html = await response.Content.ReadAsStringAsync();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("Enter the address.", html);
        await using var context = database.CreateContext();
        Assert.Equal(1, await context.Patients.CountAsync());
    }

    [Fact]
    public async Task FR_PAT_001_possible_duplicate_shows_the_warning_and_saves_nothing()
    {
        using var client = await SignedInClientAsync();
        var token = await TestSignIn.AntiForgeryTokenAsync(client, "/Patients/Create");

        var response = await client.PostAsync("/Patients/Create", Form(
            ("FullName", "Test Other Name"), ("DateOfBirth", "1995-03-03"), ("Gender", "Other"),
            ("Phone", ExistingPhone), ("Email", "other@example.test"), ("Address", "Test street 2"),
            ("__RequestVerificationToken", token)));

        var html = await response.Content.ReadAsStringAsync();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("Possible duplicate", html);
        Assert.Contains("Confirm and save", html);
        await using var context = database.CreateContext();
        Assert.Equal(1, await context.Patients.CountAsync());
    }

    [Fact]
    public async Task FR_PAT_001_possible_duplicate_is_saved_after_confirmation()
    {
        using var client = await SignedInClientAsync();
        var token = await TestSignIn.AntiForgeryTokenAsync(client, "/Patients/Create");

        var response = await client.PostAsync("/Patients/Create", Form(
            ("FullName", "Test Other Name"), ("DateOfBirth", "1995-03-03"), ("Gender", "Other"),
            ("Phone", ExistingPhone), ("Email", "other@example.test"), ("Address", "Test street 2"),
            ("ConfirmWarnings", "true"), ("__RequestVerificationToken", token)));

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        await using var context = database.CreateContext();
        Assert.Equal(2, await context.Patients.CountAsync());
    }

    [Fact]
    public async Task FR_PAT_002_edit_saves_the_change_and_the_profile_shows_it()
    {
        using var client = await SignedInClientAsync();
        var token = await TestSignIn.AntiForgeryTokenAsync(client, $"/Patients/Edit/{existingId}");

        var response = await client.PostAsync($"/Patients/Edit/{existingId}", Form(
            ("FullName", ExistingName), ("DateOfBirth", "1990-05-01"), ("Gender", "Female"),
            ("Phone", ExistingPhone), ("Email", "anna@example.test"), ("Address", "Test address changed"),
            ("__RequestVerificationToken", token)));

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        var profile = await client.GetStringAsync($"/Patients/Details/{existingId}");
        Assert.Contains("Test address changed", profile);
    }

    [Fact]
    public async Task FR_PAT_002_edit_page_shows_the_current_values_with_an_iso_date()
    {
        using var client = await SignedInClientAsync();

        var html = await client.GetStringAsync($"/Patients/Edit/{existingId}");

        Assert.Contains(ExistingName, html);
        Assert.Contains("value=\"1990-05-01\"", html);
    }

    [Fact]
    public async Task FR_PAT_003_profile_shows_every_field_and_the_age()
    {
        using var client = await SignedInClientAsync();
        var expectedAge = AgeCalculator.AgeOn(ExistingBirth, DateOnly.FromDateTime(DateTime.Now));

        var html = await client.GetStringAsync($"/Patients/Details/{existingId}");

        Assert.Contains(ExistingName, html);
        Assert.Contains("1990-05-01", html);
        Assert.Contains("Female", html);
        Assert.Contains(ExistingPhone, html);
        Assert.Contains("anna@example.test", html);
        Assert.Contains("Test address", html);
        Assert.Contains($"<dd class=\"col-sm-9\">{expectedAge}</dd>", html);
    }

    [Fact]
    public async Task FR_SRC_003_profile_links_back_to_search()
    {
        using var client = await SignedInClientAsync();

        var html = await client.GetStringAsync($"/Patients/Details/{existingId}");

        Assert.Contains("href=\"/Patients/Search\"", html);
    }

    [Fact]
    public async Task FR_PAT_004_search_results_link_to_the_profile()
    {
        using var client = await SignedInClientAsync();

        var html = await client.GetStringAsync("/Patients/Search?q=anna");

        Assert.Contains($"href=\"/Patients/Details/{existingId}\"", html);
    }

    [Fact]
    public async Task FR_PAT_004_no_match_offers_to_register_a_new_patient()
    {
        using var client = await SignedInClientAsync();

        var html = await client.GetStringAsync("/Patients/Search?q=zara");

        Assert.Contains("href=\"/Patients/Create\"", html);
    }

    [Fact]
    public async Task FR_PAT_001_register_page_offers_the_three_genders()
    {
        using var client = await SignedInClientAsync();

        var html = await client.GetStringAsync("/Patients/Create");

        Assert.Contains("value=\"Male\"", html);
        Assert.Contains("value=\"Female\"", html);
        Assert.Contains("value=\"Other\"", html);
    }

    [Fact]
    public async Task FR_PAT_003_profile_of_an_unknown_patient_is_not_found()
    {
        using var client = await SignedInClientAsync();

        var response = await client.GetAsync("/Patients/Details/999999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task FR_PAT_002_edit_of_an_unknown_patient_is_not_found()
    {
        using var client = await SignedInClientAsync();

        var response = await client.GetAsync("/Patients/Edit/999999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task NFR_SEC_001_register_post_without_an_anti_forgery_token_is_refused()
    {
        using var client = await SignedInClientAsync();

        var response = await client.PostAsync("/Patients/Create", Form(
            ("FullName", "Test Ben Example"), ("DateOfBirth", "1985-02-14"), ("Gender", "Male"),
            ("Phone", "5550199"), ("Email", "ben@example.test"), ("Address", "Test street 1")));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        await using var context = database.CreateContext();
        Assert.Equal(1, await context.Patients.CountAsync());
    }

    private async Task<HttpClient> SignedInClientAsync()
    {
        var client = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
        await TestSignIn.SignInAsync(client);
        return client;
    }

    private static FormUrlEncodedContent Form(params (string Name, string Value)[] fields) =>
        new(fields.Select(field => new KeyValuePair<string, string>(field.Name, field.Value)));
}
