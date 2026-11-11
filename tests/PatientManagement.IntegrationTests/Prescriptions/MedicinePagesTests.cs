using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using PatientManagement.Data.Entities;
using PatientManagement.IntegrationTests.Fixtures;

namespace PatientManagement.IntegrationTests.Prescriptions;

// The medicine section on the consultation screen (SCR-11) through the web app against the test database:
// saving, refusals, the printed lock and the page state. Synthetic data only. Test IDs follow CLAUDE.md §4.2 and §10.
[Collection(TestDatabaseCollection.Name)]
public class MedicinePagesTests(TestDatabaseFixture database, TestWebApplicationFactory factory)
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
    public async Task FR_RX_001_saving_medicines_keeps_their_order_and_returns_to_the_visit()
    {
        var visitId = await SeedVisitAsync();

        var reply = await PostMedicinesAsync(visitId,
            ("Medicines[0].Name", "Test Medicine"), ("Medicines[0].Dosage", "1 tablet"),
            ("Medicines[1].Name", "Test Second"));

        Assert.Equal(HttpStatusCode.Redirect, reply.Status);
        Assert.Contains($"/Consultations/Edit/{visitId}", reply.Location);
        await using var reader = database.CreateContext();
        var items = await reader.PrescriptionItems.OrderBy(item => item.SortOrder).ToListAsync();
        Assert.Equal(["Test Medicine", "Test Second"], items.Select(item => item.Name));
        Assert.Equal("1 tablet", items[0].Dosage);
    }

    [Fact]
    public async Task FR_RX_001_a_medicine_without_a_name_is_refused_and_the_typed_values_stay_on_the_page()
    {
        var visitId = await SeedVisitAsync();

        var reply = await PostMedicinesAsync(visitId, ("Medicines[0].Dosage", "Test dose"));

        Assert.Equal(HttpStatusCode.OK, reply.Status);
        Assert.Contains("enter the medicine name", reply.Body);
        Assert.Contains("Test dose", reply.Body);
        await using var reader = database.CreateContext();
        Assert.Equal(0, await reader.Prescriptions.CountAsync());
    }

    [Fact]
    public async Task FR_RX_001_more_than_twenty_medicines_are_refused()
    {
        var visitId = await SeedVisitAsync();
        var fields = Enumerable.Range(0, 21)
            .Select(index => ($"Medicines[{index}].Name", $"Test Medicine {index}"))
            .ToArray();

        var reply = await PostMedicinesAsync(visitId, fields);

        Assert.Equal(HttpStatusCode.OK, reply.Status);
        Assert.Contains("at most 20 medicines", reply.Body);
    }

    [Fact]
    public async Task FR_RX_001_removing_every_medicine_clears_the_prescription()
    {
        var visitId = await SeedVisitAsync();
        await SeedPrescriptionAsync(visitId, printed: false, "Test Old One", "Test Old Two");

        var reply = await PostMedicinesAsync(visitId);

        Assert.Equal(HttpStatusCode.Redirect, reply.Status);
        await using var reader = database.CreateContext();
        Assert.Equal(0, await reader.PrescriptionItems.CountAsync());
    }

    [Fact]
    public async Task FR_RX_001_the_consultation_screen_shows_the_saved_medicines()
    {
        var visitId = await SeedVisitAsync();
        await SeedPrescriptionAsync(visitId, printed: false, "Test Medicine");

        var page = await GetPageAsync($"/Consultations/Edit/{visitId}");

        Assert.Contains("value=\"Test Medicine\"", page);
    }

    [Fact]
    public async Task FR_RX_002_medicines_cannot_be_saved_once_printed()
    {
        var visitId = await SeedVisitAsync();
        await SeedPrescriptionAsync(visitId, printed: true, "Test Printed");

        var reply = await PostMedicinesAsync(visitId, ("Medicines[0].Name", "Test Late"));

        Assert.Equal(HttpStatusCode.OK, reply.Status);
        Assert.Contains("can no longer be changed", reply.Body);
        Assert.Contains("Test Printed", reply.Body);
        Assert.DoesNotContain("Test Late", reply.Body);
        await using var reader = database.CreateContext();
        Assert.Equal(["Test Printed"], (await reader.PrescriptionItems.ToListAsync()).Select(item => item.Name));
    }

    [Fact]
    public async Task FR_RX_002_a_printed_visit_cannot_be_saved()
    {
        var visitId = await SeedVisitAsync();
        await SeedPrescriptionAsync(visitId, printed: true, "Test Printed");

        var reply = await PostVisitAsync(visitId, ("Complaints", "Test late note"));

        Assert.Equal(HttpStatusCode.OK, reply.Status);
        Assert.Contains("printed prescription", reply.Body);
        await using var reader = database.CreateContext();
        Assert.Null((await reader.Consultations.SingleAsync()).Complaints);
    }

    [Fact]
    public async Task FR_RX_002_the_printed_visit_is_shown_as_read_only()
    {
        var visitId = await SeedVisitAsync();
        await SeedPrescriptionAsync(visitId, printed: true, "Test Printed");

        var page = await GetPageAsync($"/Consultations/Edit/{visitId}");

        Assert.Contains("has been printed", page);
        Assert.Contains("read-only", page);
    }

    private async Task<int> SeedVisitAsync()
    {
        await using var context = database.CreateContext();
        var visit = new Consultation { PatientId = patientId, VisitDateTime = Morning };
        context.Consultations.Add(visit);
        await context.SaveChangesAsync();
        return visit.Id;
    }

    private async Task SeedPrescriptionAsync(int consultationId, bool printed, params string[] names)
    {
        await using var context = database.CreateContext();
        var prescription = new Prescription
        {
            ConsultationId = consultationId,
            IsPrinted = printed,
            PrintedAt = printed ? Morning : null,
            Items = names.Select((name, index) => new PrescriptionItem
            {
                Name = name,
                SortOrder = (short)(index + 1)
            }).ToList()
        };
        context.Prescriptions.Add(prescription);
        await context.SaveChangesAsync();
    }

    private async Task<Reply> PostMedicinesAsync(int visitId, params (string Name, string Value)[] fields) =>
        await PostAsync($"/Prescriptions/SaveItems?consultationId={visitId}", $"/Consultations/Edit/{visitId}", fields);

    private async Task<Reply> PostVisitAsync(int visitId, params (string Name, string Value)[] fields) =>
        await PostAsync($"/Consultations/Edit/{visitId}", $"/Consultations/Edit/{visitId}", fields);

    private async Task<Reply> PostAsync(string url, string pageUrl, (string Name, string Value)[] fields)
    {
        using var client = await SignedInClientAsync();
        var token = await TestSignIn.AntiForgeryTokenAsync(client, pageUrl);
        var response = await client.PostAsync(url, Form(fields.Append(("__RequestVerificationToken", token)).ToArray()));

        return new Reply(response.StatusCode, await response.Content.ReadAsStringAsync(), response.Headers.Location?.ToString());
    }

    private async Task<string> GetPageAsync(string url)
    {
        using var client = await SignedInClientAsync();
        var response = await client.GetAsync(url);

        return await response.Content.ReadAsStringAsync();
    }

    private sealed record Reply(HttpStatusCode Status, string Body, string? Location);

    private async Task<HttpClient> SignedInClientAsync()
    {
        var client = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
        await TestSignIn.SignInAsync(client);
        return client;
    }

    private static FormUrlEncodedContent Form(params (string Name, string Value)[] fields) =>
        new(fields.Select(field => new KeyValuePair<string, string>(field.Name, field.Value)));

    private async Task Clear() =>
        await database.ClearTablesAsync("PrescriptionItems", "Prescriptions", "VitalSigns", "Consultations", "Appointments", "Patients");
}
