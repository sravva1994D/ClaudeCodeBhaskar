using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using PatientManagement.Data.Entities;
using PatientManagement.IntegrationTests.Fixtures;

namespace PatientManagement.IntegrationTests.Appointments;

// The schedule screen (SCR-09) and its links, through the web app against the test database. Synthetic data only.
// Test IDs follow CLAUDE.md §4.2 and §10.
[Collection(TestDatabaseCollection.Name)]
public class AppointmentPagesTests(TestDatabaseFixture database, TestWebApplicationFactory factory)
    : IClassFixture<TestWebApplicationFactory>, IAsyncLifetime
{
    // A future day, so the past-date rule does not apply to the normal cases.
    private static readonly DateOnly Day = DateOnly.FromDateTime(DateTime.Today.AddDays(30));

    private int patientId;

    public async Task InitializeAsync()
    {
        await database.ClearTablesAsync("Appointments", "Patients");
        await TestSignIn.SeedAccountAsync(database);

        await using var context = database.CreateContext();
        var patient = new Patient
        {
            FullName = "Test Anna Example",
            DateOfBirth = new DateOnly(1990, 5, 1),
            Gender = "Female",
            Phone = "5550101",
            Email = "anna@example.test",
            Address = "Test address"
        };
        context.Patients.Add(patient);
        await context.SaveChangesAsync();
        patientId = patient.Id;
    }

    public async Task DisposeAsync()
    {
        await database.ClearTablesAsync("Appointments", "Patients");
        await database.ClearTablesAsync("Users");
    }

    [Fact]
    public async Task FR_APT_001_schedule_screen_lists_the_fourteen_slots_for_the_chosen_day()
    {
        using var client = await SignedInClientAsync();

        var html = await client.GetStringAsync($"/Appointments/Create?patientId={patientId}&date={Day:yyyy-MM-dd}");

        Assert.Equal(14, CountOccurrences(html, "name=\"StartTime\""));
        Assert.Contains("Test Anna Example", html);
    }

    [Fact]
    public async Task FR_APT_001_a_booked_slot_is_shown_as_booked_and_cannot_be_chosen()
    {
        await SeedAppointmentAsync(Day.ToDateTime(new TimeOnly(9, 0)), "Scheduled");
        using var client = await SignedInClientAsync();

        var html = await client.GetStringAsync($"/Appointments/Create?patientId={patientId}&date={Day:yyyy-MM-dd}");

        Assert.Single(System.Text.RegularExpressions.Regex.Matches(html, "\\(booked\\)"));
        Assert.Single(System.Text.RegularExpressions.Regex.Matches(html, "disabled=\"disabled\""));
    }

    [Fact]
    public async Task FR_APT_001_scheduling_saves_the_appointment_and_opens_the_profile_with_a_message()
    {
        using var client = await SignedInClientAsync();
        var token = await TestSignIn.AntiForgeryTokenAsync(client, $"/Appointments/Create?patientId={patientId}&date={Day:yyyy-MM-dd}");

        var response = await client.PostAsync("/Appointments/Create", Form(
            ("PatientId", patientId.ToString()),
            ("Date", Day.ToString("yyyy-MM-dd")),
            ("StartTime", Day.ToDateTime(new TimeOnly(9, 0)).ToString("yyyy-MM-ddTHH:mm")),
            ("__RequestVerificationToken", token)));

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Contains($"/Patients/Details/{patientId}", response.Headers.Location!.ToString());

        var profile = await client.GetStringAsync($"/Patients/Details/{patientId}");
        Assert.Contains("Appointment booked", profile);

        await using var context = database.CreateContext();
        var saved = await context.Appointments.SingleAsync();
        Assert.Equal("Scheduled", saved.Status);
        Assert.Equal(30, saved.DurationMinutes);
    }

    [Fact]
    public async Task FR_APT_001_a_slot_that_is_already_booked_is_refused_on_the_form()
    {
        await SeedAppointmentAsync(Day.ToDateTime(new TimeOnly(9, 0)), "Scheduled");
        using var client = await SignedInClientAsync();
        var token = await TestSignIn.AntiForgeryTokenAsync(client, $"/Appointments/Create?patientId={patientId}&date={Day:yyyy-MM-dd}");

        var response = await client.PostAsync("/Appointments/Create", Form(
            ("PatientId", patientId.ToString()),
            ("Date", Day.ToString("yyyy-MM-dd")),
            ("StartTime", Day.ToDateTime(new TimeOnly(9, 0)).ToString("yyyy-MM-ddTHH:mm")),
            ("__RequestVerificationToken", token)));

        var html = await response.Content.ReadAsStringAsync();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("That slot is already booked.", html);
        await using var context = database.CreateContext();
        Assert.Equal(1, await context.Appointments.CountAsync());
    }

    [Fact]
    public async Task FR_APT_001_submitting_without_a_slot_shows_a_message()
    {
        using var client = await SignedInClientAsync();
        var token = await TestSignIn.AntiForgeryTokenAsync(client, $"/Appointments/Create?patientId={patientId}&date={Day:yyyy-MM-dd}");

        var response = await client.PostAsync("/Appointments/Create", Form(
            ("PatientId", patientId.ToString()),
            ("Date", Day.ToString("yyyy-MM-dd")),
            ("__RequestVerificationToken", token)));

        var html = await response.Content.ReadAsStringAsync();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("Choose a slot.", html);
    }

    [Fact]
    public async Task FR_APT_001_a_past_date_is_refused_on_the_form()
    {
        using var client = await SignedInClientAsync();
        var yesterday = DateOnly.FromDateTime(DateTime.Today.AddDays(-1));
        var token = await TestSignIn.AntiForgeryTokenAsync(client, $"/Appointments/Create?patientId={patientId}");

        var response = await client.PostAsync("/Appointments/Create", Form(
            ("PatientId", patientId.ToString()),
            ("Date", yesterday.ToString("yyyy-MM-dd")),
            ("StartTime", yesterday.ToDateTime(new TimeOnly(9, 0)).ToString("yyyy-MM-ddTHH:mm")),
            ("__RequestVerificationToken", token)));

        var html = await response.Content.ReadAsStringAsync();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("The appointment date cannot be in the past.", html);
        await using var context = database.CreateContext();
        Assert.Equal(0, await context.Appointments.CountAsync());
    }

    [Fact]
    public async Task FR_APT_001_a_slot_on_a_different_day_from_the_day_box_is_refused()
    {
        using var client = await SignedInClientAsync();
        var token = await TestSignIn.AntiForgeryTokenAsync(client, $"/Appointments/Create?patientId={patientId}&date={Day:yyyy-MM-dd}");

        // The day box says Day, but the chosen slot is on the next day (the slot list was not reloaded).
        var response = await client.PostAsync("/Appointments/Create", Form(
            ("PatientId", patientId.ToString()),
            ("Date", Day.ToString("yyyy-MM-dd")),
            ("StartTime", Day.AddDays(1).ToDateTime(new TimeOnly(9, 0)).ToString("yyyy-MM-ddTHH:mm")),
            ("__RequestVerificationToken", token)));

        var html = await response.Content.ReadAsStringAsync();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("The chosen slot is on a different day.", html);
        await using var context = database.CreateContext();
        Assert.Equal(0, await context.Appointments.CountAsync());
    }

    [Fact]
    public async Task FR_APT_001_an_unregistered_patient_gets_not_found_on_the_screen()
    {
        using var client = await SignedInClientAsync();

        var response = await client.GetAsync($"/Appointments/Create?patientId=999999&date={Day:yyyy-MM-dd}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task FR_APT_001_an_unregistered_patient_gets_not_found_on_save()
    {
        using var client = await SignedInClientAsync();
        var token = await TestSignIn.AntiForgeryTokenAsync(client, $"/Patients/Details/{patientId}");

        var response = await client.PostAsync("/Appointments/Create", Form(
            ("PatientId", "999999"),
            ("Date", Day.ToString("yyyy-MM-dd")),
            ("StartTime", Day.ToDateTime(new TimeOnly(9, 0)).ToString("yyyy-MM-ddTHH:mm")),
            ("__RequestVerificationToken", token)));

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task FR_APT_001_the_schedule_screen_needs_sign_in()
    {
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

        var response = await client.GetAsync($"/Appointments/Create?patientId={patientId}");

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        // The redirect may be an absolute URL, so compare the path only.
        Assert.Equal("/Account/Login", new Uri(new Uri("http://localhost"), response.Headers.Location!).AbsolutePath);
    }

    [Fact]
    public async Task FR_APT_001_a_post_without_an_anti_forgery_token_is_refused()
    {
        using var client = await SignedInClientAsync();

        var response = await client.PostAsync("/Appointments/Create", Form(
            ("PatientId", patientId.ToString()),
            ("Date", Day.ToString("yyyy-MM-dd")),
            ("StartTime", Day.ToDateTime(new TimeOnly(9, 0)).ToString("yyyy-MM-ddTHH:mm"))));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task FR_APT_001_the_patient_profile_links_to_the_schedule_screen()
    {
        using var client = await SignedInClientAsync();

        var html = await client.GetStringAsync($"/Patients/Details/{patientId}");

        Assert.Contains($"/Appointments/Create?patientId={patientId}", html);
    }

    [Fact]
    public async Task FR_APT_001_search_results_link_to_the_schedule_screen()
    {
        using var client = await SignedInClientAsync();

        var html = await client.GetStringAsync("/Patients/Search?q=Anna");

        Assert.Contains($"/Appointments/Create?patientId={patientId}", html);
    }

    private async Task SeedAppointmentAsync(DateTime startTime, string status)
    {
        await using var context = database.CreateContext();
        context.Appointments.Add(new Appointment
        {
            PatientId = patientId,
            StartTime = startTime,
            DurationMinutes = 30,
            Status = status,
            IsArchived = false
        });
        await context.SaveChangesAsync();
    }

    private async Task<HttpClient> SignedInClientAsync()
    {
        var client = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
        await TestSignIn.SignInAsync(client);
        return client;
    }

    private static FormUrlEncodedContent Form(params (string Name, string Value)[] fields) =>
        new(fields.Select(field => new KeyValuePair<string, string>(field.Name, field.Value)));

    private static int CountOccurrences(string text, string value) =>
        text.Split(value).Length - 1;
}
