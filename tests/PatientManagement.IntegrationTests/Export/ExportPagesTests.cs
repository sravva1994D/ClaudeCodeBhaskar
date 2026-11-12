using System.Net;
using System.Text;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using PatientManagement.Data.Entities;
using PatientManagement.IntegrationTests.Fixtures;

namespace PatientManagement.IntegrationTests.Export;

// The export form (SCR-13), the CSV download and the profile link, through the web app against the test database.
// Synthetic data only. Test IDs follow CLAUDE.md §4.2 and §10.
[Collection(TestDatabaseCollection.Name)]
public class ExportPagesTests(TestDatabaseFixture database, TestWebApplicationFactory factory)
    : IClassFixture<TestWebApplicationFactory>, IAsyncLifetime
{
    private static readonly DateTime Earlier = new(2026, 10, 2, 9, 0, 0);
    private static readonly DateTime Later = new(2026, 11, 3, 14, 30, 0);

    private int patientId;
    private string patientCode = string.Empty;

    public async Task InitializeAsync()
    {
        await Clear();
        await TestSignIn.SeedAccountAsync(database);

        await using var context = database.CreateContext();
        var anna = new Patient
        {
            FullName = "Test Anna Example",
            DateOfBirth = new DateOnly(1990, 5, 1),
            Gender = "Female",
            Phone = "5550101",
            Email = "patient@example.test",
            Address = "Test address"
        };
        context.Patients.Add(anna);
        await context.SaveChangesAsync();
        patientId = anna.Id;
        patientCode = (await context.Patients.SingleAsync(patient => patient.Id == patientId)).PatientCode;
    }

    public async Task DisposeAsync()
    {
        await Clear();
        await database.ClearTablesAsync("Users");
    }

    [Fact]
    public async Task FR_EXP_001_export_pages_need_sign_in()
    {
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

        var csv = await client.GetAsync($"/Export/Patient/{patientId}/Csv");
        var form = await client.GetAsync($"/Export/Patient/{patientId}");

        Assert.Equal(HttpStatusCode.Redirect, csv.StatusCode);
        Assert.Equal(HttpStatusCode.Redirect, form.StatusCode);
        Assert.Equal("/Account/Login", new Uri(new Uri("http://localhost"), csv.Headers.Location!).AbsolutePath);
    }

    [Fact]
    public async Task FR_EXP_001_unknown_patient_is_not_found_for_the_form_and_the_csv()
    {
        using var client = await SignedInClientAsync();

        Assert.Equal(HttpStatusCode.NotFound, (await client.GetAsync("/Export/Patient/999999")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await client.GetAsync("/Export/Patient/999999/Csv")).StatusCode);
    }

    [Fact]
    public async Task FR_EXP_001_csv_downloads_as_utf8_csv_named_with_the_patient_code()
    {
        await SeedVisitAsync(patientId, Later, complaints: "Test cough");
        using var client = await SignedInClientAsync();

        var response = await client.GetAsync($"/Export/Patient/{patientId}/Csv");
        var bytes = await response.Content.ReadAsByteArrayAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("text/csv", response.Content.Headers.ContentType!.MediaType);
        Assert.Equal("utf-8", response.Content.Headers.ContentType.CharSet);
        Assert.Equal($"patient-{patientCode}-visits.csv", response.Content.Headers.ContentDisposition!.FileName);
        Assert.Equal(new byte[] { 0xEF, 0xBB, 0xBF }, bytes[..3]);
        Assert.Contains("Test cough", Encoding.UTF8.GetString(bytes));
    }

    [Fact]
    public async Task FR_EXP_001_csv_has_the_header_and_one_row_per_visit()
    {
        await SeedVisitAsync(patientId, Earlier);
        await SeedVisitAsync(patientId, Later);
        using var client = await SignedInClientAsync();

        var text = await CsvTextAsync(client, $"/Export/Patient/{patientId}/Csv");

        var lines = text.Split("\r\n", StringSplitOptions.RemoveEmptyEntries);
        Assert.StartsWith("PatientCode,FullName,DateOfBirth", lines[0]);
        Assert.Equal(3, lines.Length);
        Assert.Contains("2026-10-02,09:00", lines[1]);
        Assert.Contains("2026-11-03,14:30", lines[2]);
    }

    [Fact]
    public async Task FR_EXP_001_csv_range_includes_both_ends_and_leaves_out_other_days()
    {
        await SeedVisitAsync(patientId, Earlier, complaints: "Test early visit");
        await SeedVisitAsync(patientId, Later, complaints: "Test late visit");
        using var client = await SignedInClientAsync();

        var text = await CsvTextAsync(client, $"/Export/Patient/{patientId}/Csv?from=2026-10-01&to=2026-10-02");

        Assert.Contains("Test early visit", text);
        Assert.DoesNotContain("Test late visit", text);
    }

    [Fact]
    public async Task FR_EXP_001_csv_for_a_patient_with_no_visits_has_one_row()
    {
        using var client = await SignedInClientAsync();

        var text = await CsvTextAsync(client, $"/Export/Patient/{patientId}/Csv");

        var lines = text.Split("\r\n", StringSplitOptions.RemoveEmptyEntries);
        Assert.Equal(2, lines.Length);
        Assert.StartsWith($"{patientCode},Test Anna Example,1990-05-01", lines[1]);
    }

    [Fact]
    public async Task FR_EXP_001_a_range_with_no_visits_shows_the_message_on_the_form_and_downloads_nothing()
    {
        await SeedVisitAsync(patientId, Earlier);
        using var client = await SignedInClientAsync();

        var response = await client.GetAsync($"/Export/Patient/{patientId}/Csv?from=2026-01-01&to=2026-01-31");
        var html = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("text/html", response.Content.Headers.ContentType!.MediaType);
        Assert.Contains("No visits in this date range.", html);
        Assert.Null(response.Content.Headers.ContentDisposition);
    }

    [Fact]
    public async Task FR_EXP_001_a_from_date_after_the_to_date_shows_the_error_and_downloads_nothing()
    {
        await SeedVisitAsync(patientId, Earlier);
        using var client = await SignedInClientAsync();

        var response = await client.GetAsync($"/Export/Patient/{patientId}/Csv?from=2026-11-05&to=2026-11-01");
        var html = await response.Content.ReadAsStringAsync();

        Assert.Contains("From date must not be after To date.", html);
        Assert.Null(response.Content.Headers.ContentDisposition);
    }

    [Fact]
    public async Task FR_EXP_001_a_to_date_without_a_from_date_shows_the_error()
    {
        using var client = await SignedInClientAsync();

        var html = await client.GetStringAsync($"/Export/Patient/{patientId}/Csv?to=2026-11-01");

        Assert.Contains("Enter a From date to filter by a range.", html);
    }

    [Fact]
    public async Task FR_EXP_001_a_malformed_date_is_refused_and_not_silently_dropped()
    {
        await SeedVisitAsync(patientId, Earlier);
        using var client = await SignedInClientAsync();

        var response = await client.GetAsync($"/Export/Patient/{patientId}/Csv?from=2026-02-30");
        var html = await response.Content.ReadAsStringAsync();

        Assert.Contains("Enter each date as a real date", html);
        Assert.Null(response.Content.Headers.ContentDisposition);
    }

    [Fact]
    public async Task FR_EXP_001_the_form_is_shown_with_the_patient_and_the_download_link()
    {
        using var client = await SignedInClientAsync();

        var html = await client.GetStringAsync($"/Export/Patient/{patientId}");

        Assert.Contains("Test Anna Example", html);
        Assert.Contains(patientCode, html);
        Assert.Contains($"/Export/Patient/{patientId}/Csv", html);
        Assert.Contains("oldest first", html);
    }

    [Fact]
    public async Task FR_EXP_001_the_patient_profile_links_to_the_export()
    {
        using var client = await SignedInClientAsync();

        var profile = await client.GetStringAsync($"/Patients/Details/{patientId}");

        Assert.Contains($"/Export/Patient/{patientId}", profile);
    }

    [Fact]
    public async Task FR_EXP_002_printable_view_shows_the_patient_visit_vitals_and_medicines()
    {
        var visit = await SeedVisitAsync(patientId, Later, complaints: "Test cough", diagnosis: "Test viral fever");
        await AddPrescriptionAsync(visit, "Test Medicine");
        using var client = await SignedInClientAsync();

        var html = await client.GetStringAsync($"/Export/Patient/{patientId}/Print");

        Assert.Contains("Test Anna Example", html);
        Assert.Contains(patientCode, html);
        Assert.Contains("Visit on 2026-11-03 14:30", html);
        Assert.Contains("Test cough", html);
        Assert.Contains("Test viral fever", html);
        Assert.Contains("Test Medicine", html);
        Assert.Contains("Vitals: not recorded.", html);
    }

    [Fact]
    public async Task FR_EXP_002_printable_view_lists_the_newest_visit_first()
    {
        await SeedVisitAsync(patientId, Earlier, complaints: "Test older visit");
        await SeedVisitAsync(patientId, Later, complaints: "Test newer visit");
        using var client = await SignedInClientAsync();

        var html = await client.GetStringAsync($"/Export/Patient/{patientId}/Print");

        Assert.True(html.IndexOf("Test newer visit", StringComparison.Ordinal) < html.IndexOf("Test older visit", StringComparison.Ordinal));
    }

    [Fact]
    public async Task FR_EXP_002_printable_view_range_shows_only_visits_inside_it()
    {
        await SeedVisitAsync(patientId, Earlier, complaints: "Test early visit");
        await SeedVisitAsync(patientId, Later, complaints: "Test late visit");
        using var client = await SignedInClientAsync();

        var html = await client.GetStringAsync($"/Export/Patient/{patientId}/Print?from=2026-10-01&to=2026-10-02");

        Assert.Contains("Test early visit", html);
        Assert.DoesNotContain("Test late visit", html);
        Assert.Contains("From 2026-10-01 to 2026-10-02", html);
    }

    [Fact]
    public async Task FR_EXP_002_printable_view_has_a_print_button_and_the_save_as_pdf_hint()
    {
        using var client = await SignedInClientAsync();

        var html = await client.GetStringAsync($"/Export/Patient/{patientId}/Print");

        Assert.Contains("window.print()", html);
        Assert.Contains("Save as PDF", html);
    }

    [Fact]
    public async Task FR_EXP_002_printable_view_with_no_visits_in_a_range_shows_the_message_on_the_form()
    {
        await SeedVisitAsync(patientId, Earlier);
        using var client = await SignedInClientAsync();

        var response = await client.GetAsync($"/Export/Patient/{patientId}/Print?from=2026-01-01&to=2026-01-31");
        var html = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("No visits in this date range.", html);
        Assert.Contains("Open printable view", html);
    }

    [Fact]
    public async Task FR_EXP_002_printing_the_view_does_not_mark_the_prescription_printed()
    {
        var visit = await SeedVisitAsync(patientId, Later);
        await AddPrescriptionAsync(visit, "Test Medicine");
        using var client = await SignedInClientAsync();

        await client.GetAsync($"/Export/Patient/{patientId}/Print");

        await using var context = database.CreateContext();
        var prescription = await context.Prescriptions.SingleAsync(p => p.ConsultationId == visit);
        Assert.False(prescription.IsPrinted);
        Assert.Null(prescription.PrintedAt);
    }

    [Fact]
    public async Task FR_EXP_002_printable_view_needs_sign_in_and_unknown_patient_is_not_found()
    {
        using var anonymous = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
        using var client = await SignedInClientAsync();

        var redirected = await anonymous.GetAsync($"/Export/Patient/{patientId}/Print");

        Assert.Equal(HttpStatusCode.Redirect, redirected.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await client.GetAsync("/Export/Patient/999999/Print")).StatusCode);
    }

    private async Task AddPrescriptionAsync(int visitId, string medicineName)
    {
        await using var context = database.CreateContext();
        context.Prescriptions.Add(new Prescription
        {
            ConsultationId = visitId,
            Items = [new PrescriptionItem { Name = medicineName, Dosage = "500 mg", SortOrder = 1 }]
        });
        await context.SaveChangesAsync();
    }

    private async Task<string> CsvTextAsync(HttpClient client, string url)
    {
        var bytes = await (await client.GetAsync(url)).Content.ReadAsByteArrayAsync();
        return Encoding.UTF8.GetString(bytes).TrimStart('﻿');
    }

    private async Task<int> SeedVisitAsync(int patient, DateTime visitTime, string? complaints = null, string? diagnosis = null)
    {
        await using var context = database.CreateContext();
        var visit = new Consultation { PatientId = patient, VisitDateTime = visitTime, Complaints = complaints, Diagnosis = diagnosis };
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
