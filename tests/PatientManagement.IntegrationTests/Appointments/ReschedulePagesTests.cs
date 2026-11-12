using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using PatientManagement.Data.Entities;
using PatientManagement.IntegrationTests.Fixtures;

namespace PatientManagement.IntegrationTests.Appointments;

// The reschedule screen (FR-APT-004, DEC-027) through the web app against the test database. Synthetic data only.
// Test IDs follow CLAUDE.md §4.2 and §10.
[Collection(TestDatabaseCollection.Name)]
public class ReschedulePagesTests(TestDatabaseFixture database, TestWebApplicationFactory factory)
    : IClassFixture<TestWebApplicationFactory>, IAsyncLifetime
{
    // A future day, so the past-date rule does not apply.
    private static readonly DateOnly Day = DateOnly.FromDateTime(DateTime.Today.AddDays(30));

    private int patientId;

    public async Task InitializeAsync()
    {
        await database.ClearTablesAsync("PrescriptionItems", "Prescriptions", "VitalSigns", "Consultations", "Appointments", "Patients");
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
        await database.ClearTablesAsync("PrescriptionItems", "Prescriptions", "VitalSigns", "Consultations", "Appointments", "Patients");
        await database.ClearTablesAsync("Users");
    }

    [Fact]
    public async Task FR_APT_004_the_reschedule_screen_opens_for_a_scheduled_appointment_and_marks_its_current_slot()
    {
        var appointmentId = await SeedAppointmentAsync(Day.ToDateTime(new TimeOnly(9, 0)), "Scheduled");
        using var client = await SignedInClientAsync();

        var html = await client.GetStringAsync($"/Appointments/Reschedule?id={appointmentId}");

        Assert.Contains("Reschedule appointment", html);
        Assert.Contains("Test Anna Example", html);
        Assert.Equal(14, CountOccurrences(html, "name=\"StartTime\""));
        Assert.Single(System.Text.RegularExpressions.Regex.Matches(html, "\\(current\\)"));
    }

    [Fact]
    public async Task FR_APT_004_moving_saves_the_new_time_and_frees_the_old_slot()
    {
        var appointmentId = await SeedAppointmentAsync(Day.ToDateTime(new TimeOnly(9, 0)), "Scheduled");
        using var client = await SignedInClientAsync();
        var token = await TestSignIn.AntiForgeryTokenAsync(client, $"/Appointments/Reschedule?id={appointmentId}");

        var response = await client.PostAsync("/Appointments/Reschedule", Form(
            ("AppointmentId", appointmentId.ToString()),
            ("PatientId", patientId.ToString()),
            ("Date", Day.ToString("yyyy-MM-dd")),
            ("StartTime", Day.ToDateTime(new TimeOnly(10, 0)).ToString("yyyy-MM-ddTHH:mm")),
            ("__RequestVerificationToken", token)));

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Contains("/Appointments/Daily", response.Headers.Location!.ToString());

        await using var context = database.CreateContext();
        var moved = await context.Appointments.SingleAsync();
        Assert.Equal(appointmentId, moved.Id);
        Assert.Equal(Day.ToDateTime(new TimeOnly(10, 0)), moved.StartTime);
        Assert.Equal("Scheduled", moved.Status);
        Assert.Equal(patientId, moved.PatientId);
    }

    [Fact]
    public async Task FR_APT_004_a_move_onto_a_booked_slot_is_refused_and_nothing_changes()
    {
        var appointmentId = await SeedAppointmentAsync(Day.ToDateTime(new TimeOnly(9, 0)), "Scheduled");
        await SeedAppointmentAsync(Day.ToDateTime(new TimeOnly(10, 0)), "Scheduled");
        using var client = await SignedInClientAsync();
        var token = await TestSignIn.AntiForgeryTokenAsync(client, $"/Appointments/Reschedule?id={appointmentId}");

        var response = await client.PostAsync("/Appointments/Reschedule", Form(
            ("AppointmentId", appointmentId.ToString()),
            ("PatientId", patientId.ToString()),
            ("Date", Day.ToString("yyyy-MM-dd")),
            ("StartTime", Day.ToDateTime(new TimeOnly(10, 0)).ToString("yyyy-MM-ddTHH:mm")),
            ("__RequestVerificationToken", token)));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var html = await response.Content.ReadAsStringAsync();
        Assert.Contains("already booked", html);

        await using var context = database.CreateContext();
        var stillThere = await context.Appointments.SingleAsync(appointment => appointment.Id == appointmentId);
        Assert.Equal(Day.ToDateTime(new TimeOnly(9, 0)), stillThere.StartTime);
    }

    [Fact]
    public async Task FR_APT_004_a_checked_in_appointment_cannot_be_rescheduled_from_the_screen()
    {
        var appointmentId = await SeedAppointmentAsync(Day.ToDateTime(new TimeOnly(9, 0)), "Checked-In");
        using var client = await SignedInClientAsync();

        var response = await client.GetAsync($"/Appointments/Reschedule?id={appointmentId}");

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Contains("/Appointments/Daily", response.Headers.Location!.ToString());
    }

    [Fact]
    public async Task FR_APT_004_the_daily_list_offers_reschedule_only_for_scheduled_appointments()
    {
        await SeedAppointmentAsync(Day.ToDateTime(new TimeOnly(9, 0)), "Scheduled");
        await SeedAppointmentAsync(Day.ToDateTime(new TimeOnly(10, 0)), "Checked-In");
        using var client = await SignedInClientAsync();

        var html = await client.GetStringAsync($"/Appointments/Daily?date={Day:yyyy-MM-dd}");

        Assert.Single(System.Text.RegularExpressions.Regex.Matches(html, "/Appointments/Reschedule/"));
    }

    private async Task<int> SeedAppointmentAsync(DateTime startTime, string status)
    {
        await using var context = database.CreateContext();
        var appointment = new Appointment
        {
            PatientId = patientId,
            StartTime = startTime,
            DurationMinutes = 30,
            Status = status,
            IsArchived = false
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

    private static int CountOccurrences(string text, string value) =>
        text.Split(value).Length - 1;
}
