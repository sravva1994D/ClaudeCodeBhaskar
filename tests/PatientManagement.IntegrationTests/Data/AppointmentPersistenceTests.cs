using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using PatientManagement.Data.Entities;
using PatientManagement.IntegrationTests.Fixtures;

namespace PatientManagement.IntegrationTests.Data;

// Data tests for the Appointments table on SQL Server: CHECK constraints, the filtered slot index (PDR-07),
// and the patient foreign key. Synthetic values only. Test IDs follow CLAUDE.md §4.2 and §10.
[Collection(TestDatabaseCollection.Name)]
public class AppointmentPersistenceTests(TestDatabaseFixture fixture) : IAsyncLifetime
{
    private static readonly DateTime Slot = new(2026, 12, 1, 9, 0, 0);

    // Children first: Appointments reference Patients.
    public async Task InitializeAsync() => await fixture.ClearTablesAsync("VitalSigns", "Consultations", "Appointments", "Patients");

    public async Task DisposeAsync() => await fixture.ClearTablesAsync("VitalSigns", "Consultations", "Appointments", "Patients");

    [Fact]
    public async Task FR_APT_001_appointment_is_saved_with_thirty_minutes_and_not_archived()
    {
        var patientId = await AddPatientAsync();
        await SaveAsync(NewAppointment(patientId, Slot));

        await using var reader = fixture.CreateContext();
        var appointment = await reader.Appointments.SingleAsync();

        Assert.Equal(30, appointment.DurationMinutes);
        Assert.False(appointment.IsArchived);
        Assert.Equal(Slot, appointment.StartTime);
    }

    [Fact]
    public async Task FR_APT_001_duration_other_than_thirty_minutes_is_refused_by_the_database()
    {
        var patientId = await AddPatientAsync();
        var appointment = NewAppointment(patientId, Slot);
        appointment.DurationMinutes = 45;

        await AssertRefusedAsync(appointment);
    }

    [Fact]
    public async Task FR_APT_003_status_outside_the_six_values_is_refused_by_the_database()
    {
        var patientId = await AddPatientAsync();

        await AssertRefusedAsync(NewAppointment(patientId, Slot, status: "Rescheduled"));
    }

    [Fact]
    public async Task FR_APT_003_each_of_the_six_statuses_is_accepted()
    {
        var patientId = await AddPatientAsync();
        var statuses = new[] { "Scheduled", "Checked-In", "In Consultation", "Completed", "Cancelled", "No-show" };

        for (var i = 0; i < statuses.Length; i++)
        {
            // A different slot per status, so the slot index does not interfere.
            await SaveAsync(NewAppointment(patientId, Slot.AddMinutes(30 * i), statuses[i]));
        }

        await using var reader = fixture.CreateContext();
        Assert.Equal(statuses.Length, await reader.Appointments.CountAsync());
    }

    [Theory]
    [InlineData("Scheduled")]
    [InlineData("Checked-In")]
    [InlineData("In Consultation")]
    [InlineData("Completed")]
    public async Task FR_APT_001_a_live_appointment_holds_its_slot(string status)
    {
        var patientId = await AddPatientAsync();
        await SaveAsync(NewAppointment(patientId, Slot, status));

        await AssertRefusedAsync(NewAppointment(patientId, Slot, "Scheduled"));
    }

    [Theory]
    [InlineData("Cancelled")]
    [InlineData("No-show")]
    public async Task FR_APT_001_cancelled_or_no_show_releases_the_slot(string status)
    {
        var patientId = await AddPatientAsync();
        await SaveAsync(NewAppointment(patientId, Slot, status));

        await SaveAsync(NewAppointment(patientId, Slot, "Scheduled"));

        await using var reader = fixture.CreateContext();
        Assert.Equal(2, await reader.Appointments.CountAsync(a => a.StartTime == Slot));
    }

    [Fact]
    public async Task FR_APT_003_archived_appointment_releases_the_slot()
    {
        var patientId = await AddPatientAsync();
        await SaveAsync(NewAppointment(patientId, Slot, "Completed", archived: true));

        await SaveAsync(NewAppointment(patientId, Slot, "Scheduled"));

        await using var reader = fixture.CreateContext();
        Assert.Equal(2, await reader.Appointments.CountAsync(a => a.StartTime == Slot));
    }

    [Fact]
    public async Task FR_APT_001_appointment_needs_an_existing_patient()
    {
        await AssertRefusedAsync(NewAppointment(patientId: 999999, Slot));
    }

    [Fact]
    public async Task FR_APT_001_patient_with_an_appointment_cannot_be_deleted()
    {
        var patientId = await AddPatientAsync();
        await SaveAsync(NewAppointment(patientId, Slot));

        await using var context = fixture.CreateContext();

        await Assert.ThrowsAsync<SqlException>(() =>
            context.Database.ExecuteSqlRawAsync("DELETE FROM [Patients] WHERE [Id] = {0}", patientId));
    }

    private async Task<int> AddPatientAsync()
    {
        await using var context = fixture.CreateContext();
        var patient = new Patient
        {
            FullName = "Test Patient Appointment",
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

    private async Task SaveAsync(Appointment appointment)
    {
        await using var context = fixture.CreateContext();
        context.Appointments.Add(appointment);
        await context.SaveChangesAsync();
    }

    private async Task AssertRefusedAsync(Appointment appointment)
    {
        await using var context = fixture.CreateContext();
        context.Appointments.Add(appointment);

        await Assert.ThrowsAsync<DbUpdateException>(() => context.SaveChangesAsync());
    }

    private static Appointment NewAppointment(int patientId, DateTime startTime, string status = "Scheduled", bool archived = false) => new()
    {
        PatientId = patientId,
        StartTime = startTime,
        DurationMinutes = 30,
        Status = status,
        IsArchived = archived
    };
}
