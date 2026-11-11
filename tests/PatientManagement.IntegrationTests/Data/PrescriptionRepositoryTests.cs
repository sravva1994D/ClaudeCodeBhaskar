using Microsoft.EntityFrameworkCore;
using PatientManagement.Data.Abstractions;
using PatientManagement.Data.Entities;
using PatientManagement.Data.Repositories;
using PatientManagement.IntegrationTests.Fixtures;

namespace PatientManagement.IntegrationTests.Data;

// Repository tests for prescriptions and medicine lines against SQL Server. Synthetic values only.
// Test IDs follow CLAUDE.md §4.2 and §10.
[Collection(TestDatabaseCollection.Name)]
public class PrescriptionRepositoryTests(TestDatabaseFixture fixture) : IAsyncLifetime
{
    private static readonly DateTime Visit = new(2026, 12, 1, 9, 0, 0);

    public async Task InitializeAsync() => await Clear();

    public async Task DisposeAsync() => await Clear();

    [Fact]
    public async Task FR_RX_001_lines_are_read_back_in_display_order()
    {
        var consultationId = await AddConsultationAsync();
        await AddPrescriptionAsync(consultationId);
        var prescriptionId = await PrescriptionIdAsync(consultationId);
        await ReplaceAsync(prescriptionId, Line("Test Second", sortOrder: 2), Line("Test First", sortOrder: 1));

        var prescription = await GetAsync(consultationId);

        Assert.NotNull(prescription);
        Assert.Equal(["Test First", "Test Second"], prescription!.Items.Select(item => item.Name));
    }

    [Fact]
    public async Task FR_RX_002_no_prescription_is_returned_for_a_consultation_without_one()
    {
        var consultationId = await AddConsultationAsync();

        Assert.Null(await GetAsync(consultationId));
    }

    [Fact]
    public async Task FR_RX_002_a_new_prescription_is_not_printed()
    {
        var consultationId = await AddConsultationAsync();
        await AddPrescriptionAsync(consultationId);

        var prescription = await GetAsync(consultationId);

        Assert.False(prescription!.IsPrinted);
        Assert.Null(prescription.PrintedAt);
    }

    [Fact]
    public async Task FR_RX_002_a_second_prescription_for_a_consultation_is_refused()
    {
        var consultationId = await AddConsultationAsync();
        await AddPrescriptionAsync(consultationId);

        await using var context = fixture.CreateContext();

        await Assert.ThrowsAsync<PrescriptionAlreadyExistsException>(() =>
            new PrescriptionRepository(context).AddAsync(new Prescription { ConsultationId = consultationId }));
    }

    [Fact]
    public async Task FR_RX_001_replacing_lines_removes_the_previous_lines()
    {
        var consultationId = await AddConsultationAsync();
        await AddPrescriptionAsync(consultationId);
        var prescriptionId = await PrescriptionIdAsync(consultationId);
        await ReplaceAsync(prescriptionId, Line("Test Old One", sortOrder: 1), Line("Test Old Two", sortOrder: 2));

        Assert.True(await ReplaceAsync(prescriptionId, Line("Test New", sortOrder: 1)));

        var prescription = await GetAsync(consultationId);
        Assert.Equal(["Test New"], prescription!.Items.Select(item => item.Name));
    }

    [Fact]
    public async Task FR_RX_001_replacing_lines_with_none_removes_them_all()
    {
        var consultationId = await AddConsultationAsync();
        await AddPrescriptionAsync(consultationId);
        var prescriptionId = await PrescriptionIdAsync(consultationId);
        await ReplaceAsync(prescriptionId, Line("Test Old", sortOrder: 1));

        Assert.True(await ReplaceAsync(prescriptionId));

        Assert.Empty((await GetAsync(consultationId))!.Items);
    }

    [Fact]
    public async Task FR_RX_002_lines_cannot_be_replaced_after_printing()
    {
        var consultationId = await AddConsultationAsync();
        await AddPrescriptionAsync(consultationId);
        var prescriptionId = await PrescriptionIdAsync(consultationId);
        await ReplaceAsync(prescriptionId, Line("Test Printed", sortOrder: 1));
        Assert.True(await PrintAsync(prescriptionId));

        Assert.False(await ReplaceAsync(prescriptionId, Line("Test Late", sortOrder: 1)));

        var prescription = await GetAsync(consultationId);
        Assert.Equal(["Test Printed"], prescription!.Items.Select(item => item.Name));
    }

    [Fact]
    public async Task FR_RX_002_lines_of_a_missing_prescription_are_not_saved()
    {
        Assert.False(await ReplaceAsync(999999, Line("Test Orphan", sortOrder: 1)));
    }

    [Fact]
    public async Task FR_RX_002_print_marks_the_prescription_printed()
    {
        var consultationId = await AddConsultationAsync();
        await AddPrescriptionAsync(consultationId);
        var prescriptionId = await PrescriptionIdAsync(consultationId);
        await ReplaceAsync(prescriptionId, Line("Test Medicine", sortOrder: 1));

        Assert.True(await PrintAsync(prescriptionId, Visit.AddMinutes(30)));

        var prescription = await GetAsync(consultationId);
        Assert.True(prescription!.IsPrinted);
        Assert.Equal(Visit.AddMinutes(30), prescription.PrintedAt);
    }

    [Fact]
    public async Task FR_RX_002_a_second_print_changes_nothing()
    {
        var consultationId = await AddConsultationAsync();
        await AddPrescriptionAsync(consultationId);
        var prescriptionId = await PrescriptionIdAsync(consultationId);
        await ReplaceAsync(prescriptionId, Line("Test Medicine", sortOrder: 1));
        Assert.True(await PrintAsync(prescriptionId, Visit.AddMinutes(30)));

        Assert.False(await PrintAsync(prescriptionId, Visit.AddMinutes(45)));

        Assert.Equal(Visit.AddMinutes(30), (await GetAsync(consultationId))!.PrintedAt);
    }

    [Fact]
    public async Task FR_RX_002_a_prescription_without_lines_cannot_be_printed()
    {
        var consultationId = await AddConsultationAsync();
        await AddPrescriptionAsync(consultationId);
        var prescriptionId = await PrescriptionIdAsync(consultationId);

        Assert.False(await PrintAsync(prescriptionId));

        Assert.False((await GetAsync(consultationId))!.IsPrinted);
    }

    private async Task<int> AddConsultationAsync()
    {
        await using var context = fixture.CreateContext();
        var patient = new Patient
        {
            FullName = "Test Patient Prescription",
            DateOfBirth = new DateOnly(1980, 1, 1),
            Gender = "Female",
            Phone = "5550100",
            Email = "patient@example.test",
            Address = "Test address"
        };
        context.Patients.Add(patient);
        await context.SaveChangesAsync();

        var consultation = new Consultation { PatientId = patient.Id, VisitDateTime = Visit };
        context.Consultations.Add(consultation);
        await context.SaveChangesAsync();
        return consultation.Id;
    }

    private async Task AddPrescriptionAsync(int consultationId)
    {
        await using var context = fixture.CreateContext();
        await new PrescriptionRepository(context).AddAsync(new Prescription { ConsultationId = consultationId });
    }

    private async Task<int> PrescriptionIdAsync(int consultationId)
    {
        await using var context = fixture.CreateContext();
        return (await context.Prescriptions.SingleAsync(prescription => prescription.ConsultationId == consultationId)).Id;
    }

    private async Task<bool> ReplaceAsync(int prescriptionId, params PrescriptionItem[] items)
    {
        await using var context = fixture.CreateContext();
        return await new PrescriptionRepository(context).ReplaceItemsAsync(prescriptionId, items);
    }

    private async Task<bool> PrintAsync(int prescriptionId, DateTime? printedAt = null)
    {
        await using var context = fixture.CreateContext();
        return await new PrescriptionRepository(context).MarkPrintedAsync(prescriptionId, printedAt ?? Visit.AddMinutes(30));
    }

    private async Task<Prescription?> GetAsync(int consultationId)
    {
        await using var context = fixture.CreateContext();
        return await new PrescriptionRepository(context).GetByConsultationAsync(consultationId);
    }

    private static PrescriptionItem Line(string name, short sortOrder) => new()
    {
        Name = name,
        SortOrder = sortOrder
    };

    private async Task Clear() =>
        await fixture.ClearTablesAsync("PrescriptionItems", "Prescriptions", "VitalSigns", "Consultations", "Appointments", "Patients");
}
