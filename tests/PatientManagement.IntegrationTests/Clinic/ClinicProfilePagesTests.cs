using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using PatientManagement.IntegrationTests.Fixtures;

namespace PatientManagement.IntegrationTests.Clinic;

// The clinic details page (SCR-14) through the web app against the test database: sign-in, saving, refusals and the
// navigation link. Synthetic data only. Test IDs follow CLAUDE.md §4.2 and §10.
[Collection(TestDatabaseCollection.Name)]
public class ClinicProfilePagesTests(TestDatabaseFixture database, TestWebApplicationFactory factory)
    : IClassFixture<TestWebApplicationFactory>, IAsyncLifetime
{
    private const string Page = "/ClinicProfile/Edit";

    public async Task InitializeAsync()
    {
        await Clear();
        await TestSignIn.SeedAccountAsync(database);
    }

    public async Task DisposeAsync()
    {
        await Clear();
        await database.ClearTablesAsync("Users");
    }

    [Fact]
    public async Task FR_RX_002_the_clinic_details_page_opens_for_a_signed_in_user_and_is_in_the_menu()
    {
        using var client = await SignedInClientAsync();

        var response = await client.GetAsync(Page);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("Clinic name", body);
        Assert.Contains($"href=\"{Page}\"", body);
    }

    [Fact]
    public async Task FR_RX_002_the_clinic_details_page_needs_sign_in()
    {
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

        var response = await client.GetAsync(Page);

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
    }

    [Fact]
    public async Task FR_RX_002_saved_clinic_details_are_stored_trimmed()
    {
        using var client = await SignedInClientAsync();

        var response = await PostAsync(client,
            ("ClinicName", "  Test Clinic  "), ("Address", "Test address"), ("Phone", "5550199"),
            ("DoctorName", "Test Doctor"), ("RegistrationNumber", "TEST-1"));

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Contains(Page, response.Headers.Location!.ToString());
        await using var reader = database.CreateContext();
        var saved = await reader.ClinicProfiles.SingleAsync();
        Assert.Equal("Test Clinic", saved.ClinicName);
        Assert.Equal("TEST-1", saved.RegistrationNumber);
    }

    [Fact]
    public async Task FR_RX_002_a_second_save_updates_the_same_single_row()
    {
        using var client = await SignedInClientAsync();
        await PostAsync(client, ("ClinicName", "Test Clinic"));

        await PostAsync(client, ("ClinicName", "Test Clinic Renamed"));

        await using var reader = database.CreateContext();
        var rows = await reader.ClinicProfiles.ToListAsync();
        Assert.Equal("Test Clinic Renamed", Assert.Single(rows).ClinicName);
    }

    [Fact]
    public async Task FR_RX_002_a_field_longer_than_its_column_is_refused_and_nothing_is_saved()
    {
        using var client = await SignedInClientAsync();

        var response = await PostAsync(client, ("Phone", new string('5', 21)));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("at most 20 characters", await response.Content.ReadAsStringAsync());
        // The blank row exists because the page was read (D-M5-6); the refused value must not be in it.
        await using var reader = database.CreateContext();
        Assert.Null((await reader.ClinicProfiles.SingleAsync()).Phone);
    }

    [Fact]
    public async Task FR_RX_002_a_blank_header_is_saved_and_the_print_notice_is_left_to_the_print_view()
    {
        using var client = await SignedInClientAsync();

        var response = await PostAsync(client, ("ClinicName", ""), ("Phone", "   "));

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        await using var reader = database.CreateContext();
        var saved = await reader.ClinicProfiles.SingleAsync();
        Assert.Null(saved.ClinicName);
        Assert.Null(saved.Phone);
    }

    private async Task<HttpResponseMessage> PostAsync(HttpClient client, params (string Name, string Value)[] fields)
    {
        var token = await TestSignIn.AntiForgeryTokenAsync(client, Page);

        var form = fields.Select(field => new KeyValuePair<string, string>(field.Name, field.Value)).ToList();
        form.Add(new KeyValuePair<string, string>("__RequestVerificationToken", token));

        return await client.PostAsync(Page, new FormUrlEncodedContent(form));
    }

    private async Task<HttpClient> SignedInClientAsync()
    {
        var client = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
        await TestSignIn.SignInAsync(client);
        return client;
    }

    private async Task Clear() => await database.ClearTablesAsync("ClinicProfile");
}
