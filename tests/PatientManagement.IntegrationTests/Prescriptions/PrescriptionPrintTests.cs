using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using PatientManagement.Data.Entities;
using PatientManagement.IntegrationTests.Fixtures;

namespace PatientManagement.IntegrationTests.Prescriptions;

// The printable prescription (SCR-12) and the Print command, and the medicines on visit detail (SCR-08), through the web app
// against the test database. Synthetic data only. Test IDs follow CLAUDE.md §4.2 and §10.
[Collection(TestDatabaseCollection.Name)]
public class PrescriptionPrintTests(TestDatabaseFixture database, TestWebApplicationFactory factory)
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
    public async Task FR_RX_002_the_print_view_shows_the_header_patient_vitals_diagnosis_and_medicines()
    {
        await SeedClinicAsync(complete: true);
        var visitId = await SeedVisitAsync(withVitals: true, diagnosis: "Test diagnosis");
        await SeedPrescriptionAsync(visitId, printed: false, ("Test Medicine", "1 tablet"), ("Test Second", null));

        var reply = await GetAsync($"/Prescriptions/Print/{visitId}");

        Assert.Equal(HttpStatusCode.OK, reply.Status);
        Assert.Contains("Test Clinic", reply.Body);
        Assert.Contains("Test Anna Example", reply.Body);
        Assert.Contains("01-May-1990", reply.Body);
        Assert.Contains("120/80", reply.Body);
        Assert.Contains("Test diagnosis", reply.Body);
        Assert.Contains("Test Medicine", reply.Body);
        Assert.Contains("1 tablet", reply.Body);
        Assert.Contains("Test Second", reply.Body);
        Assert.DoesNotContain("window.print()", reply.Body);
    }

    [Fact]
    public async Task FR_RX_002_the_print_view_says_when_the_clinic_details_are_incomplete()
    {
        var visitId = await SeedVisitAsync(withVitals: false, diagnosis: null);
        await SeedPrescriptionAsync(visitId, printed: false, ("Test Medicine", null));

        var reply = await GetAsync($"/Prescriptions/Print/{visitId}");

        Assert.Contains("Clinic details are incomplete", reply.Body);
    }

    [Fact]
    public async Task FR_RX_002_the_print_view_is_not_available_without_a_medicine()
    {
        var visitId = await SeedVisitAsync(withVitals: false, diagnosis: null);

        var reply = await GetAsync($"/Prescriptions/Print/{visitId}");

        Assert.Equal(HttpStatusCode.Redirect, reply.Status);
        Assert.Contains($"/Consultations/Edit/{visitId}", reply.Location);
    }

    [Fact]
    public async Task FR_RX_002_printing_marks_the_prescription_printed_and_opens_the_print_dialog()
    {
        await SeedClinicAsync(complete: true);
        var visitId = await SeedVisitAsync(withVitals: false, diagnosis: null);
        await SeedPrescriptionAsync(visitId, printed: false, ("Test Medicine", null));

        var reply = await PrintAsync(visitId);

        Assert.Equal(HttpStatusCode.OK, reply.Status);
        Assert.Contains("window.print()", reply.Body);
        await using var reader = database.CreateContext();
        var prescription = await reader.Prescriptions.SingleAsync();
        Assert.True(prescription.IsPrinted);
        Assert.NotNull(prescription.PrintedAt);
    }

    [Fact]
    public async Task FR_RX_002_printing_again_is_allowed_and_keeps_the_first_print_time()
    {
        await SeedClinicAsync(complete: true);
        var visitId = await SeedVisitAsync(withVitals: false, diagnosis: null);
        await SeedPrescriptionAsync(visitId, printed: false, ("Test Medicine", null));
        await PrintAsync(visitId);
        DateTime firstPrint;
        await using (var reader = database.CreateContext())
        {
            firstPrint = (await reader.Prescriptions.SingleAsync()).PrintedAt!.Value;
        }

        var reply = await PrintAsync(visitId);

        Assert.Equal(HttpStatusCode.OK, reply.Status);
        await using var after = database.CreateContext();
        Assert.Equal(firstPrint, (await after.Prescriptions.SingleAsync()).PrintedAt);
    }

    [Fact]
    public async Task FR_RX_002_the_print_command_needs_the_anti_forgery_token()
    {
        var visitId = await SeedVisitAsync(withVitals: false, diagnosis: null);
        await SeedPrescriptionAsync(visitId, printed: false, ("Test Medicine", null));
        using var client = await SignedInClientAsync();

        var response = await client.PostAsync($"/Prescriptions/Print/{visitId}", new FormUrlEncodedContent(
            Array.Empty<KeyValuePair<string, string>>()));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        await using var reader = database.CreateContext();
        Assert.False((await reader.Prescriptions.SingleAsync()).IsPrinted);
    }

    [Fact]
    public async Task FR_RX_002_a_visit_without_a_medicine_cannot_be_printed()
    {
        var visitId = await SeedVisitAsync(withVitals: false, diagnosis: null);

        var reply = await PrintAsync(visitId);

        Assert.Equal(HttpStatusCode.Redirect, reply.Status);
        Assert.Contains($"/Consultations/Edit/{visitId}", reply.Location);
        await using var reader = database.CreateContext();
        Assert.Equal(0, await reader.Prescriptions.CountAsync());
    }

    [Fact]
    public async Task FR_HIS_002_visit_detail_shows_the_medicines_and_a_print_link()
    {
        var visitId = await SeedVisitAsync(withVitals: false, diagnosis: null);
        await SeedPrescriptionAsync(visitId, printed: false, ("Test Medicine", "1 tablet"));

        var reply = await GetAsync($"/Visits/Details/{visitId}");

        Assert.Equal(HttpStatusCode.OK, reply.Status);
        Assert.Contains("Test Medicine", reply.Body);
        Assert.Contains($"/Prescriptions/Print/{visitId}", reply.Body);
        Assert.Contains("Not printed yet.", reply.Body);
    }

    [Fact]
    public async Task FR_HIS_002_visit_detail_without_a_medicine_says_so()
    {
        var visitId = await SeedVisitAsync(withVitals: false, diagnosis: null);

        var reply = await GetAsync($"/Visits/Details/{visitId}");

        Assert.Contains("No medicines recorded.", reply.Body);
        Assert.DoesNotContain($"/Prescriptions/Print/{visitId}", reply.Body);
    }

    // The token comes from the visit page, which exists whether or not the visit has a medicine.
    private async Task<Reply> PrintAsync(int visitId)
    {
        using var client = await SignedInClientAsync();
        var token = await TestSignIn.AntiForgeryTokenAsync(client, $"/Visits/Details/{visitId}");
        var response = await client.PostAsync($"/Prescriptions/Print/{visitId}", new FormUrlEncodedContent(
            new[] { new KeyValuePair<string, string>("__RequestVerificationToken", token) }));

        return new Reply(response.StatusCode, await response.Content.ReadAsStringAsync(), response.Headers.Location?.ToString());
    }

    private async Task<Reply> GetAsync(string url)
    {
        using var client = await SignedInClientAsync();
        var response = await client.GetAsync(url);

        return new Reply(response.StatusCode, await response.Content.ReadAsStringAsync(), response.Headers.Location?.ToString());
    }

    private async Task<int> SeedVisitAsync(bool withVitals, string? diagnosis)
    {
        await using var context = database.CreateContext();
        var visit = new Consultation
        {
            PatientId = patientId,
            VisitDateTime = Morning,
            Diagnosis = diagnosis,
            Vitals = withVitals
                ? new VitalSign { TemperatureC = 36.8m, SystolicMmHg = 120, DiastolicMmHg = 80, PulseBpm = 72 }
                : null
        };
        context.Consultations.Add(visit);
        await context.SaveChangesAsync();
        return visit.Id;
    }

    private async Task SeedPrescriptionAsync(int consultationId, bool printed, params (string Name, string? Dosage)[] lines)
    {
        await using var context = database.CreateContext();
        context.Prescriptions.Add(new Prescription
        {
            ConsultationId = consultationId,
            IsPrinted = printed,
            PrintedAt = printed ? Morning : null,
            Items = lines.Select((line, index) => new PrescriptionItem
            {
                Name = line.Name,
                Dosage = line.Dosage,
                SortOrder = (short)(index + 1)
            }).ToList()
        });
        await context.SaveChangesAsync();
    }

    private async Task SeedClinicAsync(bool complete)
    {
        await using var context = database.CreateContext();
        context.ClinicProfiles.Add(new ClinicProfile
        {
            Id = 1,
            ClinicName = "Test Clinic",
            Address = complete ? "Test address" : null,
            Phone = complete ? "5550199" : null,
            DoctorName = complete ? "Test Doctor" : null,
            RegistrationNumber = complete ? "TEST-1" : null
        });
        await context.SaveChangesAsync();
    }

    private sealed record Reply(HttpStatusCode Status, string Body, string? Location);

    private async Task<HttpClient> SignedInClientAsync()
    {
        var client = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
        await TestSignIn.SignInAsync(client);
        return client;
    }

    private async Task Clear() =>
        await database.ClearTablesAsync("PrescriptionItems", "Prescriptions", "VitalSigns", "Consultations", "Appointments", "Patients", "ClinicProfile");
}
