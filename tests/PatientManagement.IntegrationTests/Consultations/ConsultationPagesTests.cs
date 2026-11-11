using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using PatientManagement.Data.Entities;
using PatientManagement.IntegrationTests.Fixtures;

namespace PatientManagement.IntegrationTests.Consultations;

// The consultation screen (SCR-11), its start entry points and Finish, through the web app against the test database.
// Synthetic data only. Test IDs follow CLAUDE.md §4.2 and §10.
[Collection(TestDatabaseCollection.Name)]
public class ConsultationPagesTests(TestDatabaseFixture database, TestWebApplicationFactory factory)
    : IClassFixture<TestWebApplicationFactory>, IAsyncLifetime
{
    private static readonly DateTime Morning = DateTime.Today.AddHours(9);

    private int patientId;

    public async Task InitializeAsync()
    {
        await Clear();
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
        await Clear();
        await database.ClearTablesAsync("Users");
    }

    [Fact]
    public async Task FR_HIS_001_start_from_the_profile_opens_the_consultation_screen()
    {
        using var client = await SignedInClientAsync();
        var token = await TestSignIn.AntiForgeryTokenAsync(client, $"/Patients/Details/{patientId}");

        var response = await client.PostAsync("/Consultations/Start", Form(
            ("patientId", patientId.ToString()), ("__RequestVerificationToken", token)));

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Contains("/Consultations/Edit/", response.Headers.Location!.ToString());
        await using var context = database.CreateContext();
        var visit = await context.Consultations.SingleAsync();
        Assert.Null(visit.AppointmentId);
    }

    [Fact]
    public async Task FR_APT_003_start_from_a_checked_in_appointment_sets_it_to_in_consultation()
    {
        var appointmentId = await SeedAppointmentAsync("Checked-In");
        using var client = await SignedInClientAsync();
        var token = await TestSignIn.AntiForgeryTokenAsync(client, $"/Patients/Details/{patientId}");

        var response = await client.PostAsync("/Consultations/Start", Form(
            ("patientId", patientId.ToString()), ("appointmentId", appointmentId.ToString()),
            ("__RequestVerificationToken", token)));

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        await using var context = database.CreateContext();
        Assert.Equal("In Consultation", (await context.Appointments.SingleAsync()).Status);
        Assert.Equal(appointmentId, (await context.Consultations.SingleAsync()).AppointmentId);
    }

    [Fact]
    public async Task FR_APT_003_start_from_a_scheduled_appointment_is_refused_with_a_message()
    {
        var appointmentId = await SeedAppointmentAsync("Scheduled");
        using var client = await SignedInClientAsync();
        var token = await TestSignIn.AntiForgeryTokenAsync(client, $"/Appointments/Daily");

        await client.PostAsync("/Consultations/Start", Form(
            ("patientId", patientId.ToString()), ("appointmentId", appointmentId.ToString()),
            ("__RequestVerificationToken", token)));

        var list = await client.GetStringAsync("/Appointments/Daily");
        Assert.Contains("This appointment is Scheduled.", list);
        await using var context = database.CreateContext();
        Assert.Equal(0, await context.Consultations.CountAsync());
    }

    [Fact]
    public async Task FR_APT_003_refused_start_from_a_list_day_returns_to_that_day()
    {
        var appointmentId = await SeedAppointmentAsync("Scheduled");
        using var client = await SignedInClientAsync();
        var token = await TestSignIn.AntiForgeryTokenAsync(client, "/Appointments/Daily");

        // A day other than today, so the test shows the posted day was used and not the fallback to today.
        var day = new DateOnly(2026, 12, 1);

        var response = await client.PostAsync("/Consultations/Start", Form(
            ("patientId", patientId.ToString()), ("appointmentId", appointmentId.ToString()),
            ("date", day.ToString("yyyy-MM-dd")), ("__RequestVerificationToken", token)));

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        var location = new Uri(new Uri("http://localhost"), response.Headers.Location!);
        Assert.Equal("/Appointments/Daily", location.AbsolutePath);
        var redirectedDay = System.Web.HttpUtility.ParseQueryString(location.Query)["date"];
        Assert.Equal(day, DateOnly.ParseExact(redirectedDay!, "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture));
    }

    [Fact]
    public async Task FR_VIT_001_save_records_the_vitals_complaints_and_diagnosis()
    {
        var visitId = await SeedVisitAsync();
        using var client = await SignedInClientAsync();
        var token = await TestSignIn.AntiForgeryTokenAsync(client, $"/Consultations/Edit/{visitId}");

        var response = await client.PostAsync($"/Consultations/Edit/{visitId}", Form(
            ("Complaints", "Test cough"), ("Diagnosis", "Test diagnosis"),
            ("TemperatureC", "36.8"), ("SystolicMmHg", "120"), ("DiastolicMmHg", "80"), ("PulseBpm", "72"),
            ("__RequestVerificationToken", token)));

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        var page = await client.GetStringAsync($"/Consultations/Edit/{visitId}");
        Assert.Contains("Visit saved.", page);
        Assert.Contains("Test cough", page);
        Assert.Contains("value=\"120\"", page);
        await using var context = database.CreateContext();
        Assert.Equal(120, (await context.VitalSigns.SingleAsync()).SystolicMmHg);
    }

    [Fact]
    public async Task FR_VIT_001_out_of_range_vitals_need_confirmation_on_the_screen()
    {
        var visitId = await SeedVisitAsync();
        using var client = await SignedInClientAsync();
        var token = await TestSignIn.AntiForgeryTokenAsync(client, $"/Consultations/Edit/{visitId}");
        var values = new[] { ("TemperatureC", "43.0"), ("SystolicMmHg", "120"), ("DiastolicMmHg", "80"), ("PulseBpm", "72") };

        var first = await client.PostAsync($"/Consultations/Edit/{visitId}", Form(
            values.Concat([("__RequestVerificationToken", token)]).ToArray()));

        var html = await first.Content.ReadAsStringAsync();
        Assert.Equal(HttpStatusCode.OK, first.StatusCode);
        Assert.Contains("Temperature 43.0", html);
        Assert.Contains("is outside the expected range", html);
        Assert.Contains("Confirm and save", html);
        await using (var context = database.CreateContext())
        {
            Assert.Equal(0, await context.VitalSigns.CountAsync());
        }

        var confirmed = await client.PostAsync($"/Consultations/Edit/{visitId}", Form(
            values.Concat([("ConfirmWarnings", "true"), ("__RequestVerificationToken", token)]).ToArray()));

        Assert.Equal(HttpStatusCode.Redirect, confirmed.StatusCode);
        await using var saved = database.CreateContext();
        Assert.Equal(43.0m, (await saved.VitalSigns.SingleAsync()).TemperatureC);
    }

    [Fact]
    public async Task FR_VIT_001_missing_vitals_are_saved_with_a_notice()
    {
        var visitId = await SeedVisitAsync();
        using var client = await SignedInClientAsync();
        var token = await TestSignIn.AntiForgeryTokenAsync(client, $"/Consultations/Edit/{visitId}");

        var response = await client.PostAsync($"/Consultations/Edit/{visitId}", Form(
            ("Complaints", "Test cough"), ("__RequestVerificationToken", token)));

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        var page = await client.GetStringAsync($"/Consultations/Edit/{visitId}");
        Assert.Contains("No vitals recorded. You can add them later.", page);
        await using var context = database.CreateContext();
        Assert.Equal(0, await context.VitalSigns.CountAsync());
    }

    [Fact]
    public async Task FR_VIT_001_partial_vitals_are_refused_on_the_screen()
    {
        var visitId = await SeedVisitAsync();
        using var client = await SignedInClientAsync();
        var token = await TestSignIn.AntiForgeryTokenAsync(client, $"/Consultations/Edit/{visitId}");

        var response = await client.PostAsync($"/Consultations/Edit/{visitId}", Form(
            ("TemperatureC", "36.8"), ("SystolicMmHg", "120"), ("__RequestVerificationToken", token)));

        var html = await response.Content.ReadAsStringAsync();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("Enter all four vitals", html);
        await using var context = database.CreateContext();
        Assert.Equal(0, await context.VitalSigns.CountAsync());
    }

    [Fact]
    public async Task FR_VIT_001_vitals_can_be_added_on_a_later_save()
    {
        var visitId = await SeedVisitAsync();
        using var client = await SignedInClientAsync();
        var token = await TestSignIn.AntiForgeryTokenAsync(client, $"/Consultations/Edit/{visitId}");
        await client.PostAsync($"/Consultations/Edit/{visitId}", Form(("Complaints", "Test cough"), ("__RequestVerificationToken", token)));

        var later = await client.PostAsync($"/Consultations/Edit/{visitId}", Form(
            ("TemperatureC", "37.0"), ("SystolicMmHg", "118"), ("DiastolicMmHg", "78"), ("PulseBpm", "70"),
            ("__RequestVerificationToken", token)));

        Assert.Equal(HttpStatusCode.Redirect, later.StatusCode);
        await using var context = database.CreateContext();
        Assert.Equal(118, (await context.VitalSigns.SingleAsync()).SystolicMmHg);
    }

    [Fact]
    public async Task FR_APT_003_finish_completes_the_appointment_visit()
    {
        var appointmentId = await SeedAppointmentAsync("In Consultation");
        var visitId = await SeedVisitAsync(appointmentId);
        using var client = await SignedInClientAsync();
        var token = await TestSignIn.AntiForgeryTokenAsync(client, $"/Consultations/Edit/{visitId}");

        var response = await client.PostAsync($"/Consultations/Finish/{visitId}", Form(("__RequestVerificationToken", token)));

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        var page = await client.GetStringAsync($"/Consultations/Edit/{visitId}");
        Assert.Contains("Visit finished.", page);
        Assert.Contains("Finished", page);
        await using var context = database.CreateContext();
        Assert.Equal("Completed", (await context.Appointments.SingleAsync()).Status);
        Assert.NotNull((await context.Consultations.SingleAsync()).FinishedAt);
    }

    [Fact]
    public async Task FR_APT_003_finish_is_refused_when_the_appointment_is_not_in_consultation()
    {
        var appointmentId = await SeedAppointmentAsync("Checked-In");
        var visitId = await SeedVisitAsync(appointmentId);
        using var client = await SignedInClientAsync();
        var token = await TestSignIn.AntiForgeryTokenAsync(client, $"/Consultations/Edit/{visitId}");

        await client.PostAsync($"/Consultations/Finish/{visitId}", Form(("__RequestVerificationToken", token)));

        var page = await client.GetStringAsync($"/Consultations/Edit/{visitId}");
        Assert.Contains("The appointment is Checked-In.", page);
        await using var context = database.CreateContext();
        Assert.Equal("Checked-In", (await context.Appointments.SingleAsync()).Status);
    }

    [Fact]
    public async Task FR_HIS_001_finish_twice_is_refused()
    {
        var visitId = await SeedVisitAsync();
        using var client = await SignedInClientAsync();
        var token = await TestSignIn.AntiForgeryTokenAsync(client, $"/Consultations/Edit/{visitId}");
        await client.PostAsync($"/Consultations/Finish/{visitId}", Form(("__RequestVerificationToken", token)));

        await client.PostAsync($"/Consultations/Finish/{visitId}", Form(("__RequestVerificationToken", token)));

        var page = await client.GetStringAsync($"/Consultations/Edit/{visitId}");
        Assert.Contains("This visit is already finished.", page);
    }

    [Fact]
    public async Task FR_HIS_002_unknown_visit_is_not_found()
    {
        using var client = await SignedInClientAsync();

        var response = await client.GetAsync("/Consultations/Edit/999999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task FR_APT_003_daily_list_offers_start_only_for_checked_in_rows()
    {
        await SeedAppointmentAsync("Checked-In", Morning);
        await SeedAppointmentAsync("Scheduled", Morning.AddMinutes(30));
        using var client = await SignedInClientAsync();

        var html = await client.GetStringAsync("/Appointments/Daily");

        Assert.Equal(1, CountOccurrences(html, "Start consultation"));
    }

    [Fact]
    public async Task FR_VIT_001_a_save_without_an_anti_forgery_token_is_refused()
    {
        var visitId = await SeedVisitAsync();
        using var client = await SignedInClientAsync();

        var response = await client.PostAsync($"/Consultations/Edit/{visitId}", Form(("Complaints", "Test cough")));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task FR_HIS_001_the_consultation_screen_needs_sign_in()
    {
        var visitId = await SeedVisitAsync();
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

        var response = await client.GetAsync($"/Consultations/Edit/{visitId}");

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Equal("/Account/Login", new Uri(new Uri("http://localhost"), response.Headers.Location!).AbsolutePath);
    }

    private async Task<int> SeedAppointmentAsync(string status, DateTime? startTime = null)
    {
        await using var context = database.CreateContext();
        var appointment = new Appointment
        {
            PatientId = patientId,
            StartTime = startTime ?? Morning,
            DurationMinutes = 30,
            Status = status,
            IsArchived = false
        };
        context.Appointments.Add(appointment);
        await context.SaveChangesAsync();
        return appointment.Id;
    }

    private async Task<int> SeedVisitAsync(int? appointmentId = null)
    {
        await using var context = database.CreateContext();
        var visit = new Consultation
        {
            PatientId = patientId,
            AppointmentId = appointmentId,
            VisitDateTime = Morning
        };
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

    private static FormUrlEncodedContent Form(params (string Name, string Value)[] fields) =>
        new(fields.Select(field => new KeyValuePair<string, string>(field.Name, field.Value)));

    private static int CountOccurrences(string text, string value) =>
        text.Split(value).Length - 1;
}
