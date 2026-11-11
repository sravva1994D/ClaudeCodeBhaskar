using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using PatientManagement.Data.Entities;
using PatientManagement.IntegrationTests.Fixtures;

namespace PatientManagement.IntegrationTests.Data;

// Data tests for the Consultations and VitalSigns tables on SQL Server: the appointment link (PDR-26),
// walk-ins (OQ-006), vitals with no database range check (AD-14), and the patient foreign key.
// Synthetic values only. Test IDs follow CLAUDE.md §4.2 and §10.
[Collection(TestDatabaseCollection.Name)]
public class ConsultationPersistenceTests(TestDatabaseFixture fixture) : IAsyncLifetime
{
    private static readonly DateTime Visit = new(2026, 12, 1, 9, 0, 0);

    // Children first: VitalSigns → Consultations → Appointments → Patients.
    public async Task InitializeAsync() => await Clear();

    public async Task DisposeAsync() => await Clear();

    [Fact]
    public async Task FR_HIS_001_a_walk_in_consultation_without_an_appointment_is_saved()
    {
        var patientId = await AddPatientAsync();
        await SaveAsync(NewConsultation(patientId, appointmentId: null));

        await using var reader = fixture.CreateContext();
        var saved = await reader.Consultations.SingleAsync();
        Assert.Null(saved.AppointmentId);
        Assert.Equal(Visit, saved.VisitDateTime);
    }

    [Fact]
    public async Task FR_HIS_001_a_patient_can_have_several_walk_in_consultations()
    {
        var patientId = await AddPatientAsync();
        await SaveAsync(NewConsultation(patientId, appointmentId: null));
        await SaveAsync(NewConsultation(patientId, appointmentId: null));

        await using var reader = fixture.CreateContext();
        Assert.Equal(2, await reader.Consultations.CountAsync());
    }

    [Fact]
    public async Task FR_HIS_001_only_one_consultation_can_be_linked_to_an_appointment()
    {
        var patientId = await AddPatientAsync();
        var appointmentId = await AddAppointmentAsync(patientId);
        await SaveAsync(NewConsultation(patientId, appointmentId));

        await AssertRefusedAsync(NewConsultation(patientId, appointmentId));
    }

    [Fact]
    public async Task FR_HIS_001_a_consultation_needs_an_existing_patient()
    {
        await AssertRefusedAsync(NewConsultation(patientId: 999999, appointmentId: null));
    }

    [Fact]
    public async Task FR_HIS_001_a_consultation_links_only_to_an_existing_appointment()
    {
        var patientId = await AddPatientAsync();

        await AssertRefusedAsync(NewConsultation(patientId, appointmentId: 999999));
    }

    [Fact]
    public async Task FR_HIS_001_a_patient_with_a_visit_cannot_be_deleted()
    {
        var patientId = await AddPatientAsync();
        await SaveAsync(NewConsultation(patientId, appointmentId: null));

        await using var context = fixture.CreateContext();

        await Assert.ThrowsAsync<SqlException>(() =>
            context.Database.ExecuteSqlRawAsync("DELETE FROM [Patients] WHERE [Id] = {0}", patientId));
    }

    [Fact]
    public async Task FR_HIS_001_removing_an_appointment_keeps_the_visit()
    {
        var patientId = await AddPatientAsync();
        var appointmentId = await AddAppointmentAsync(patientId);
        await SaveAsync(NewConsultation(patientId, appointmentId));

        await using (var context = fixture.CreateContext())
        {
            await context.Database.ExecuteSqlRawAsync("DELETE FROM [Appointments] WHERE [Id] = {0}", appointmentId);
        }

        await using var reader = fixture.CreateContext();
        var visit = await reader.Consultations.SingleAsync();
        Assert.Null(visit.AppointmentId);
    }

    [Fact]
    public async Task FR_APT_003_finished_time_is_empty_until_finish_sets_it()
    {
        var patientId = await AddPatientAsync();
        await SaveAsync(NewConsultation(patientId, appointmentId: null));

        await using (var context = fixture.CreateContext())
        {
            var visit = await context.Consultations.SingleAsync();
            Assert.Null(visit.FinishedAt);
            visit.FinishedAt = Visit.AddMinutes(20);
            await context.SaveChangesAsync();
        }

        await using var reader = fixture.CreateContext();
        Assert.Equal(Visit.AddMinutes(20), (await reader.Consultations.SingleAsync()).FinishedAt);
    }

    [Fact]
    public async Task FR_VIT_001_vitals_are_saved_once_per_consultation()
    {
        var patientId = await AddPatientAsync();
        var consultationId = await SaveAsync(NewConsultation(patientId, appointmentId: null));
        await SaveVitalsAsync(consultationId);

        await AssertVitalsRefusedAsync(consultationId);
    }

    [Fact]
    public async Task FR_VIT_001_temperature_keeps_one_decimal_place()
    {
        var patientId = await AddPatientAsync();
        var consultationId = await SaveAsync(NewConsultation(patientId, appointmentId: null));
        await SaveVitalsAsync(consultationId, temperature: 37.5m);

        await using var reader = fixture.CreateContext();
        var vitals = await reader.VitalSigns.SingleAsync();
        Assert.Equal(37.5m, vitals.TemperatureC);
    }

    [Fact]
    public async Task FR_VIT_001_the_database_does_not_refuse_out_of_range_vitals()
    {
        // AD-14: ranges are warnings in the Business layer. The database accepts any value of the column type.
        var patientId = await AddPatientAsync();
        var consultationId = await SaveAsync(NewConsultation(patientId, appointmentId: null));
        await SaveVitalsAsync(consultationId, temperature: 45.0m, systolic: 300, diastolic: 5, pulse: 400);

        await using var reader = fixture.CreateContext();
        var vitals = await reader.VitalSigns.SingleAsync();
        Assert.Equal(300, vitals.SystolicMmHg);
    }

    [Fact]
    public async Task FR_VIT_001_vitals_are_read_with_their_visit()
    {
        var patientId = await AddPatientAsync();
        var consultationId = await SaveAsync(NewConsultation(patientId, appointmentId: null));
        await SaveVitalsAsync(consultationId);

        await using var reader = fixture.CreateContext();
        var visit = await reader.Consultations.Include(c => c.Vitals).SingleAsync();
        Assert.NotNull(visit.Vitals);
        Assert.Equal(consultationId, visit.Vitals!.ConsultationId);
    }

    private async Task Clear() =>
        await fixture.ClearTablesAsync("PrescriptionItems", "Prescriptions", "VitalSigns", "Consultations", "Appointments", "Patients");

    private async Task<int> AddPatientAsync()
    {
        await using var context = fixture.CreateContext();
        var patient = new Patient
        {
            FullName = "Test Patient Visit",
            DateOfBirth = new DateOnly(1980, 1, 1),
            Gender = "Female",
            Phone = "5550100",
            Email = "patient@example.test",
            Address = "Test address"
        };
        context.Patients.Add(patient);
        await context.SaveChangesAsync();
        return patient.Id;
    }

    private async Task<int> AddAppointmentAsync(int patientId)
    {
        await using var context = fixture.CreateContext();
        var appointment = new Appointment
        {
            PatientId = patientId,
            StartTime = Visit,
            DurationMinutes = 30,
            Status = "Checked-In",
            IsArchived = false
        };
        context.Appointments.Add(appointment);
        await context.SaveChangesAsync();
        return appointment.Id;
    }

    private async Task<int> SaveAsync(Consultation consultation)
    {
        await using var context = fixture.CreateContext();
        context.Consultations.Add(consultation);
        await context.SaveChangesAsync();
        return consultation.Id;
    }

    private async Task SaveVitalsAsync(
        int consultationId, decimal temperature = 36.8m, short systolic = 120, short diastolic = 80, short pulse = 72)
    {
        await using var context = fixture.CreateContext();
        context.VitalSigns.Add(new VitalSign
        {
            ConsultationId = consultationId,
            TemperatureC = temperature,
            SystolicMmHg = systolic,
            DiastolicMmHg = diastolic,
            PulseBpm = pulse
        });
        await context.SaveChangesAsync();
    }

    private async Task AssertVitalsRefusedAsync(int consultationId)
    {
        await using var context = fixture.CreateContext();
        context.VitalSigns.Add(new VitalSign
        {
            ConsultationId = consultationId,
            TemperatureC = 36.8m,
            SystolicMmHg = 120,
            DiastolicMmHg = 80,
            PulseBpm = 72
        });

        await Assert.ThrowsAsync<DbUpdateException>(() => context.SaveChangesAsync());
    }

    private async Task AssertRefusedAsync(Consultation consultation)
    {
        await using var context = fixture.CreateContext();
        context.Consultations.Add(consultation);

        await Assert.ThrowsAsync<DbUpdateException>(() => context.SaveChangesAsync());
    }

    private static Consultation NewConsultation(int patientId, int? appointmentId) => new()
    {
        PatientId = patientId,
        AppointmentId = appointmentId,
        VisitDateTime = Visit,
        Complaints = "Test complaint",
        Diagnosis = "Test diagnosis"
    };
}
