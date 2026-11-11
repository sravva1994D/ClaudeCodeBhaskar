using Microsoft.EntityFrameworkCore;
using PatientManagement.Data.Entities;
using PatientManagement.Data.Repositories;
using PatientManagement.IntegrationTests.Fixtures;

namespace PatientManagement.IntegrationTests.Data;

// Repository tests for vital signs, saved with the visit in one save (D-M5-8) against SQL Server. Synthetic values only.
// Test IDs follow CLAUDE.md §4.2 and §10.
[Collection(TestDatabaseCollection.Name)]
public class VitalSignRepositoryTests(TestDatabaseFixture fixture) : IAsyncLifetime
{
    private static readonly DateTime Morning = new(2026, 12, 1, 9, 0, 0);

    public async Task InitializeAsync() => await Clear();

    public async Task DisposeAsync() => await Clear();

    [Fact]
    public async Task FR_VIT_001_consultation_without_vitals_has_no_vitals_row()
    {
        var consultationId = await AddVisitAsync();

        await using var context = fixture.CreateContext();

        Assert.Null(await context.VitalSigns.SingleOrDefaultAsync(vitals => vitals.ConsultationId == consultationId));
    }

    [Fact]
    public async Task FR_VIT_001_first_save_adds_the_vitals_row()
    {
        var consultationId = await AddVisitAsync();

        Assert.True(await SaveVitalsAsync(consultationId, temperature: 37.2m));

        await using var reader = fixture.CreateContext();
        var saved = await reader.VitalSigns.SingleOrDefaultAsync(vitals => vitals.ConsultationId == consultationId);
        Assert.Equal(37.2m, saved!.TemperatureC);
        Assert.Equal(1, await reader.VitalSigns.CountAsync());
    }

    [Fact]
    public async Task FR_VIT_001_later_save_updates_the_same_row()
    {
        var consultationId = await AddVisitAsync();
        await SaveVitalsAsync(consultationId, temperature: 37.2m);

        await SaveVitalsAsync(consultationId, temperature: 38.9m, systolic: 135);

        await using var reader = fixture.CreateContext();
        var single = Assert.Single(await reader.VitalSigns.ToListAsync());
        Assert.Equal(38.9m, single.TemperatureC);
        Assert.Equal(135, single.SystolicMmHg);
    }

    [Fact]
    public async Task FR_VIT_001_out_of_range_vitals_are_saved_without_refusal()
    {
        // The range check is a warning in the Business layer (AD-14, FR-VIT-001 AC-5). The repository saves what it is given.
        var consultationId = await AddVisitAsync();

        await SaveVitalsAsync(consultationId, temperature: 43.0m, systolic: 260);

        await using var reader = fixture.CreateContext();
        var saved = await reader.VitalSigns.SingleOrDefaultAsync(vitals => vitals.ConsultationId == consultationId);
        Assert.Equal(43.0m, saved!.TemperatureC);
        Assert.Equal(260, saved.SystolicMmHg);
    }

    // Saves the visit with the given vitals, the way the consultation service does: the tracked visit is changed and saved once.
    private async Task<bool> SaveVitalsAsync(int consultationId, decimal temperature, short systolic = 120)
    {
        await using var context = fixture.CreateContext();
        var visit = await context.Consultations.Include(consultation => consultation.Vitals)
            .SingleAsync(consultation => consultation.Id == consultationId);

        visit.Vitals ??= new VitalSign { ConsultationId = consultationId };
        visit.Vitals.TemperatureC = temperature;
        visit.Vitals.SystolicMmHg = systolic;
        visit.Vitals.DiastolicMmHg = 80;
        visit.Vitals.PulseBpm = 72;

        return await new ConsultationRepository(context).UpdateAsync(visit);
    }

    private async Task<int> AddVisitAsync()
    {
        await using var context = fixture.CreateContext();
        var patient = new Patient
        {
            FullName = "Test Patient Vitals",
            DateOfBirth = new DateOnly(1980, 1, 1),
            Gender = "Female",
            Phone = "5550100",
            Email = "patient@example.test",
            Address = "Test address"
        };
        context.Patients.Add(patient);
        await context.SaveChangesAsync();

        var visit = new Consultation { PatientId = patient.Id, VisitDateTime = Morning };
        context.Consultations.Add(visit);
        await context.SaveChangesAsync();
        return visit.Id;
    }

    private async Task Clear() =>
        await fixture.ClearTablesAsync("PrescriptionItems", "Prescriptions", "VitalSigns", "Consultations", "Appointments", "Patients");
}
