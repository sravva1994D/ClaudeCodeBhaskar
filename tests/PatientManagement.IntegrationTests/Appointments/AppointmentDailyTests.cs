using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using PatientManagement.Data.Entities;
using PatientManagement.IntegrationTests.Fixtures;

namespace PatientManagement.IntegrationTests.Appointments;

// The daily list (SCR-10) and its status and archive actions, through the web app against the test database.
// Synthetic data only. Test IDs follow CLAUDE.md §4.2 and §10.
[Collection(TestDatabaseCollection.Name)]
public class AppointmentDailyTests(TestDatabaseFixture database, TestWebApplicationFactory factory)
    : IClassFixture<TestWebApplicationFactory>, IAsyncLifetime
{
    private static readonly DateOnly Day = DateOnly.FromDateTime(DateTime.Today.AddDays(30));

    private int annaId;
    private int benId;

    public async Task InitializeAsync()
    {
        await database.ClearTablesAsync("PrescriptionItems", "Prescriptions", "VitalSigns", "Consultations", "Appointments", "Patients");
        await TestSignIn.SeedAccountAsync(database);

        await using var context = database.CreateContext();
        var anna = NewPatient("Test Anna Example", "5550101");
        var ben = NewPatient("Test Ben Example", "5550102");
        context.Patients.AddRange(anna, ben);
        await context.SaveChangesAsync();
        annaId = anna.Id;
        benId = ben.Id;
    }

    public async Task DisposeAsync()
    {
        await database.ClearTablesAsync("PrescriptionItems", "Prescriptions", "VitalSigns", "Consultations", "Appointments", "Patients");
        await database.ClearTablesAsync("Users");
    }

    [Fact]
    public async Task FR_APT_002_daily_list_shows_only_the_chosen_day()
    {
        await SeedAsync(annaId, Day.ToDateTime(new TimeOnly(9, 0)), "Scheduled");
        await SeedAsync(benId, Day.AddDays(1).ToDateTime(new TimeOnly(10, 0)), "Scheduled");
        using var client = await SignedInClientAsync();

        var html = await client.GetStringAsync($"/Appointments/Daily?date={Day:yyyy-MM-dd}");

        Assert.Contains("Test Anna Example", html);
        Assert.DoesNotContain("Test Ben Example", html);
    }

    [Fact]
    public async Task FR_APT_002_archived_appointments_are_hidden_from_the_list()
    {
        await SeedAsync(annaId, Day.ToDateTime(new TimeOnly(9, 0)), "Completed", archived: true);
        using var client = await SignedInClientAsync();

        var html = await client.GetStringAsync($"/Appointments/Daily?date={Day:yyyy-MM-dd}");

        Assert.DoesNotContain("Test Anna Example", html);
        Assert.Contains("No appointments on this day.", html);
    }

    [Fact]
    public async Task FR_APT_002_the_list_shows_time_patient_code_and_status()
    {
        await SeedAsync(annaId, Day.ToDateTime(new TimeOnly(9, 30)), "Checked-In");
        using var client = await SignedInClientAsync();

        var html = await client.GetStringAsync($"/Appointments/Daily?date={Day:yyyy-MM-dd}");

        Assert.Contains("09:30", html);
        Assert.Contains($"PT-{annaId:D6}", html);
        Assert.Contains("Checked-In", html);
    }

    [Fact]
    public async Task FR_APT_003_a_scheduled_row_offers_only_the_manual_moves()
    {
        await SeedAsync(annaId, Day.ToDateTime(new TimeOnly(9, 0)), "Scheduled");
        using var client = await SignedInClientAsync();

        var html = await client.GetStringAsync($"/Appointments/Daily?date={Day:yyyy-MM-dd}");

        Assert.Contains("value=\"Checked-In\"", html);
        Assert.Contains("value=\"Cancelled\"", html);
        Assert.Contains("value=\"No-show\"", html);
        Assert.DoesNotContain("value=\"In Consultation\"", html);
        Assert.DoesNotContain("value=\"Completed\"", html);
        Assert.DoesNotContain(">Archive<", html);
    }

    [Fact]
    public async Task FR_APT_003_a_completed_row_offers_archive_and_no_status_change()
    {
        await SeedAsync(annaId, Day.ToDateTime(new TimeOnly(9, 0)), "Completed");
        using var client = await SignedInClientAsync();

        var html = await client.GetStringAsync($"/Appointments/Daily?date={Day:yyyy-MM-dd}");

        Assert.Contains(">Archive<", html);
        Assert.DoesNotContain("value=\"Checked-In\"", html);
    }

    [Fact]
    public async Task FR_APT_003_an_allowed_status_change_is_saved_and_reported()
    {
        var id = await SeedAsync(annaId, Day.ToDateTime(new TimeOnly(9, 0)), "Scheduled");
        using var client = await SignedInClientAsync();
        var token = await TestSignIn.AntiForgeryTokenAsync(client, $"/Appointments/Daily?date={Day:yyyy-MM-dd}");

        var response = await client.PostAsync("/Appointments/UpdateStatus", Form(
            ("id", id.ToString()), ("status", "Checked-In"), ("date", Day.ToString("yyyy-MM-dd")),
            ("__RequestVerificationToken", token)));

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        var list = await client.GetStringAsync($"/Appointments/Daily?date={Day:yyyy-MM-dd}");
        Assert.Contains("Appointment status set to Checked-In.", list);
        await using var context = database.CreateContext();
        Assert.Equal("Checked-In", (await context.Appointments.SingleAsync()).Status);
    }

    [Fact]
    public async Task FR_APT_003_a_disallowed_status_change_is_refused_and_not_saved()
    {
        var id = await SeedAsync(annaId, Day.ToDateTime(new TimeOnly(9, 0)), "Completed");
        using var client = await SignedInClientAsync();
        var token = await TestSignIn.AntiForgeryTokenAsync(client, $"/Appointments/Daily?date={Day:yyyy-MM-dd}");

        await client.PostAsync("/Appointments/UpdateStatus", Form(
            ("id", id.ToString()), ("status", "Scheduled"), ("date", Day.ToString("yyyy-MM-dd")),
            ("__RequestVerificationToken", token)));

        var list = await client.GetStringAsync($"/Appointments/Daily?date={Day:yyyy-MM-dd}");
        Assert.Contains("cannot change from Completed to Scheduled.", list);
        await using var context = database.CreateContext();
        Assert.Equal("Completed", (await context.Appointments.SingleAsync()).Status);
    }

    [Fact]
    public async Task FR_APT_003_archive_hides_a_completed_appointment_and_keeps_its_status()
    {
        var id = await SeedAsync(annaId, Day.ToDateTime(new TimeOnly(9, 0)), "Completed");
        using var client = await SignedInClientAsync();
        var token = await TestSignIn.AntiForgeryTokenAsync(client, $"/Appointments/Daily?date={Day:yyyy-MM-dd}");

        var response = await client.PostAsync("/Appointments/Archive", Form(
            ("id", id.ToString()), ("date", Day.ToString("yyyy-MM-dd")),
            ("__RequestVerificationToken", token)));

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        var list = await client.GetStringAsync($"/Appointments/Daily?date={Day:yyyy-MM-dd}");
        Assert.DoesNotContain("Test Anna Example", list);
        await using var context = database.CreateContext();
        var saved = await context.Appointments.SingleAsync();
        Assert.True(saved.IsArchived);
        Assert.Equal("Completed", saved.Status);
    }

    [Fact]
    public async Task FR_APT_003_a_scheduled_appointment_cannot_be_archived()
    {
        var id = await SeedAsync(annaId, Day.ToDateTime(new TimeOnly(9, 0)), "Scheduled");
        using var client = await SignedInClientAsync();
        var token = await TestSignIn.AntiForgeryTokenAsync(client, $"/Appointments/Daily?date={Day:yyyy-MM-dd}");

        await client.PostAsync("/Appointments/Archive", Form(
            ("id", id.ToString()), ("date", Day.ToString("yyyy-MM-dd")),
            ("__RequestVerificationToken", token)));

        var list = await client.GetStringAsync($"/Appointments/Daily?date={Day:yyyy-MM-dd}");
        Assert.Contains("cannot be archived", list);
        await using var context = database.CreateContext();
        Assert.False((await context.Appointments.SingleAsync()).IsArchived);
    }

    [Fact]
    public async Task FR_APT_003_a_status_change_on_an_unknown_appointment_is_not_found()
    {
        using var client = await SignedInClientAsync();
        var token = await TestSignIn.AntiForgeryTokenAsync(client, $"/Appointments/Daily?date={Day:yyyy-MM-dd}");

        var response = await client.PostAsync("/Appointments/UpdateStatus", Form(
            ("id", "999999"), ("status", "Checked-In"), ("date", Day.ToString("yyyy-MM-dd")),
            ("__RequestVerificationToken", token)));

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task FR_APT_003_a_status_change_without_an_anti_forgery_token_is_refused()
    {
        var id = await SeedAsync(annaId, Day.ToDateTime(new TimeOnly(9, 0)), "Scheduled");
        using var client = await SignedInClientAsync();

        var response = await client.PostAsync("/Appointments/UpdateStatus", Form(
            ("id", id.ToString()), ("status", "Checked-In"), ("date", Day.ToString("yyyy-MM-dd"))));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        await using var context = database.CreateContext();
        Assert.Equal("Scheduled", (await context.Appointments.SingleAsync()).Status);
    }

    [Fact]
    public async Task FR_APT_002_the_daily_list_needs_sign_in()
    {
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

        var response = await client.GetAsync("/Appointments/Daily");

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

    private async Task<int> SeedAsync(int patientId, DateTime startTime, string status, bool archived = false)
    {
        await using var context = database.CreateContext();
        var appointment = new Appointment
        {
            PatientId = patientId,
            StartTime = startTime,
            DurationMinutes = 30,
            Status = status,
            IsArchived = archived
        };
        context.Appointments.Add(appointment);
        await context.SaveChangesAsync();
        return appointment.Id;
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
