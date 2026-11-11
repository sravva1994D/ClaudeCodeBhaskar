using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using PatientManagement.Data.Entities;
using PatientManagement.IntegrationTests.Fixtures;

namespace PatientManagement.IntegrationTests.Data;

// Data tests for Prescriptions and PrescriptionItems on SQL Server: one prescription per consultation (FR-RX-002 AC-3),
// the medicine name rule and length (FR-RX-001, PDR-13), and the cascade from prescription to medicine lines.
// Synthetic values only. Test IDs follow CLAUDE.md §4.2 and §10.
[Collection(TestDatabaseCollection.Name)]
public class PrescriptionPersistenceTests(TestDatabaseFixture fixture) : IAsyncLifetime
{
    private static readonly DateTime Visit = new(2026, 12, 1, 9, 0, 0);

    // Children first: PrescriptionItems, Prescriptions, VitalSigns, Consultations, Appointments, Patients.
    public async Task InitializeAsync() => await Clear();

    public async Task DisposeAsync() => await Clear();

    [Fact]
    public async Task FR_RX_002_a_new_prescription_is_not_printed()
    {
        var consultationId = await AddConsultationAsync();
        await SaveAsync(NewPrescription(consultationId));

        await using var reader = fixture.CreateContext();
        var saved = await reader.Prescriptions.SingleAsync();
        Assert.False(saved.IsPrinted);
        Assert.Null(saved.PrintedAt);
    }

    [Fact]
    public async Task FR_RX_002_only_one_prescription_can_belong_to_a_consultation()
    {
        var consultationId = await AddConsultationAsync();
        await SaveAsync(NewPrescription(consultationId));

        await AssertPrescriptionRefusedAsync(NewPrescription(consultationId));
    }

    [Fact]
    public async Task FR_RX_002_a_prescription_needs_an_existing_consultation()
    {
        await AssertPrescriptionRefusedAsync(NewPrescription(consultationId: 999999));
    }

    [Fact]
    public async Task FR_RX_001_a_medicine_line_needs_a_name()
    {
        var consultationId = await AddConsultationAsync();
        var prescriptionId = await SaveAsync(NewPrescription(consultationId));

        await AssertItemRefusedAsync(NewItem(prescriptionId, name: null!));
    }

    [Fact]
    public async Task FR_RX_001_a_medicine_name_is_limited_to_200_characters()
    {
        var consultationId = await AddConsultationAsync();
        var prescriptionId = await SaveAsync(NewPrescription(consultationId));

        await AssertItemRefusedAsync(NewItem(prescriptionId, name: new string('A', 201)));
    }

    [Fact]
    public async Task FR_RX_001_a_medicine_name_of_200_characters_is_saved()
    {
        var consultationId = await AddConsultationAsync();
        var prescriptionId = await SaveAsync(NewPrescription(consultationId));
        await SaveItemAsync(NewItem(prescriptionId, name: new string('A', 200)));

        await using var reader = fixture.CreateContext();
        Assert.Equal(200, (await reader.PrescriptionItems.SingleAsync()).Name.Length);
    }

    [Fact]
    public async Task FR_RX_001_optional_medicine_fields_may_be_empty()
    {
        var consultationId = await AddConsultationAsync();
        var prescriptionId = await SaveAsync(NewPrescription(consultationId));
        await SaveItemAsync(new PrescriptionItem { PrescriptionId = prescriptionId, Name = "Test Medicine", SortOrder = 1 });

        await using var reader = fixture.CreateContext();
        var saved = await reader.PrescriptionItems.SingleAsync();
        Assert.Null(saved.Dosage);
        Assert.Null(saved.Frequency);
        Assert.Null(saved.Duration);
        Assert.Null(saved.Instructions);
    }

    [Fact]
    public async Task FR_RX_001_medicine_lines_are_removed_with_their_prescription()
    {
        var consultationId = await AddConsultationAsync();
        var prescriptionId = await SaveAsync(NewPrescription(consultationId));
        await SaveItemAsync(NewItem(prescriptionId, name: "Test Medicine One"));
        await SaveItemAsync(NewItem(prescriptionId, name: "Test Medicine Two"));

        await using (var context = fixture.CreateContext())
        {
            var prescription = await context.Prescriptions.SingleAsync();
            context.Prescriptions.Remove(prescription);
            await context.SaveChangesAsync();
        }

        await using var reader = fixture.CreateContext();
        Assert.Equal(0, await reader.PrescriptionItems.CountAsync());
    }

    [Fact]
    public async Task FR_RX_002_a_consultation_with_a_prescription_cannot_be_deleted()
    {
        var consultationId = await AddConsultationAsync();
        await SaveAsync(NewPrescription(consultationId));

        await using var context = fixture.CreateContext();

        await Assert.ThrowsAsync<SqlException>(() =>
            context.Database.ExecuteSqlRawAsync("DELETE FROM [Consultations] WHERE [Id] = {0}", consultationId));
    }

    private async Task Clear() =>
        await fixture.ClearTablesAsync("PrescriptionItems", "Prescriptions", "VitalSigns", "Consultations", "Appointments", "Patients");

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

    private async Task<int> SaveAsync(Prescription prescription)
    {
        await using var context = fixture.CreateContext();
        context.Prescriptions.Add(prescription);
        await context.SaveChangesAsync();
        return prescription.Id;
    }

    private async Task SaveItemAsync(PrescriptionItem item)
    {
        await using var context = fixture.CreateContext();
        context.PrescriptionItems.Add(item);
        await context.SaveChangesAsync();
    }

    private async Task AssertPrescriptionRefusedAsync(Prescription prescription)
    {
        await using var context = fixture.CreateContext();
        context.Prescriptions.Add(prescription);

        await Assert.ThrowsAsync<DbUpdateException>(() => context.SaveChangesAsync());
    }

    private async Task AssertItemRefusedAsync(PrescriptionItem item)
    {
        await using var context = fixture.CreateContext();
        context.PrescriptionItems.Add(item);

        await Assert.ThrowsAsync<DbUpdateException>(() => context.SaveChangesAsync());
    }

    private static Prescription NewPrescription(int consultationId) => new()
    {
        ConsultationId = consultationId
    };

    private static PrescriptionItem NewItem(int prescriptionId, string name) => new()
    {
        PrescriptionId = prescriptionId,
        Name = name,
        Dosage = "Test dosage",
        Frequency = "Test frequency",
        Duration = "Test duration",
        Instructions = "Test instructions",
        SortOrder = 1
    };
}
