using Microsoft.EntityFrameworkCore;
using PatientManagement.Data.Abstractions;
using PatientManagement.Data.Entities;
using PatientManagement.Data.Repositories;
using PatientManagement.IntegrationTests.Fixtures;

namespace PatientManagement.IntegrationTests.Data;

// Repository tests for appointments against SQL Server. Synthetic values only. Test IDs follow CLAUDE.md §4.2 and §10.
[Collection(TestDatabaseCollection.Name)]
public class AppointmentRepositoryTests(TestDatabaseFixture fixture) : IAsyncLifetime
{
    private static readonly DateOnly Day = new(2026, 12, 1);
    private static readonly DateTime NineAm = new(2026, 12, 1, 9, 0, 0);

    public async Task InitializeAsync() => await fixture.ClearTablesAsync("PrescriptionItems", "Prescriptions", "VitalSigns", "Consultations", "Appointments", "Patients");

    public async Task DisposeAsync() => await fixture.ClearTablesAsync("PrescriptionItems", "Prescriptions", "VitalSigns", "Consultations", "Appointments", "Patients");

    [Fact]
    public async Task FR_APT_002_daily_list_shows_only_the_chosen_day_in_time_order()
    {
        var patientId = await AddPatientAsync("Test Anna Example", "5550101");
        await SeedAsync(
            Appt(patientId, NineAm.AddMinutes(30)),
            Appt(patientId, NineAm),
            Appt(patientId, NineAm.AddDays(1)),
            Appt(patientId, NineAm.AddDays(-1)));

        var list = await DailyAsync(Day);

        Assert.Equal([NineAm, NineAm.AddMinutes(30)], list.Select(a => a.StartTime));
    }

    [Fact]
    public async Task FR_APT_002_daily_list_includes_the_whole_day_and_not_the_next_midnight()
    {
        var patientId = await AddPatientAsync("Test Anna Example", "5550101");
        var lastSlot = new DateTime(2026, 12, 1, 19, 30, 0);
        var nextMidnight = new DateTime(2026, 12, 2, 0, 0, 0);
        await SeedAsync(Appt(patientId, lastSlot), Appt(patientId, nextMidnight));

        var list = await DailyAsync(Day);

        Assert.Equal([lastSlot], list.Select(a => a.StartTime));
    }

    [Fact]
    public async Task FR_APT_002_daily_list_hides_archived_appointments()
    {
        var patientId = await AddPatientAsync("Test Anna Example", "5550101");
        await SeedAsync(
            Appt(patientId, NineAm, status: "Completed", archived: true),
            Appt(patientId, NineAm.AddMinutes(30)));

        var list = await DailyAsync(Day);

        Assert.Equal([NineAm.AddMinutes(30)], list.Select(a => a.StartTime));
    }

    [Fact]
    public async Task FR_APT_002_daily_list_carries_the_patient_name_and_code()
    {
        var patientId = await AddPatientAsync("Test Anna Example", "5550101");
        await SeedAsync(Appt(patientId, NineAm));

        var row = Assert.Single(await DailyAsync(Day));

        Assert.Equal("Test Anna Example", row.Patient.FullName);
        Assert.Equal($"PT-{patientId:D6}", row.Patient.PatientCode);
    }

    [Fact]
    public async Task FR_APT_001_overlap_is_found_for_a_live_appointment_at_the_same_start()
    {
        var patientId = await AddPatientAsync("Test Anna Example", "5550101");
        await SeedAsync(Appt(patientId, NineAm, status: "Checked-In"));

        Assert.True(await OverlapAsync(NineAm));
    }

    [Theory]
    [InlineData("Cancelled")]
    [InlineData("No-show")]
    public async Task FR_APT_001_cancelled_or_no_show_does_not_block_the_slot(string status)
    {
        var patientId = await AddPatientAsync("Test Anna Example", "5550101");
        await SeedAsync(Appt(patientId, NineAm, status));

        Assert.False(await OverlapAsync(NineAm));
    }

    [Fact]
    public async Task FR_APT_001_archived_appointment_does_not_block_the_slot()
    {
        var patientId = await AddPatientAsync("Test Anna Example", "5550101");
        await SeedAsync(Appt(patientId, NineAm, status: "Completed", archived: true));

        Assert.False(await OverlapAsync(NineAm));
    }

    [Fact]
    public async Task FR_APT_001_overlap_check_ignores_the_appointment_being_changed()
    {
        var patientId = await AddPatientAsync("Test Anna Example", "5550101");
        var id = await SeedAsync(Appt(patientId, NineAm));

        Assert.False(await OverlapAsync(NineAm, excludeId: id));
        Assert.True(await OverlapAsync(NineAm, excludeId: id + 1));
    }

    [Fact]
    public async Task FR_APT_001_free_slot_reports_no_overlap()
    {
        var patientId = await AddPatientAsync("Test Anna Example", "5550101");
        await SeedAsync(Appt(patientId, NineAm));

        Assert.False(await OverlapAsync(NineAm.AddMinutes(30)));
    }

    [Fact]
    public async Task FR_APT_001_added_appointment_is_saved()
    {
        var patientId = await AddPatientAsync("Test Anna Example", "5550101");

        await using (var context = fixture.CreateContext())
        {
            await new AppointmentRepository(context).AddAsync(Appt(patientId, NineAm));
        }

        await using var reader = fixture.CreateContext();
        var saved = await reader.Appointments.SingleAsync();
        Assert.Equal(NineAm, saved.StartTime);
    }

    [Fact]
    public async Task FR_APT_001_a_second_live_appointment_in_the_same_slot_raises_slot_taken()
    {
        var patientId = await AddPatientAsync("Test Anna Example", "5550101");
        await SeedAsync(Appt(patientId, NineAm));

        await using var context = fixture.CreateContext();
        var repository = new AppointmentRepository(context);

        await Assert.ThrowsAsync<SlotTakenException>(() => repository.AddAsync(Appt(patientId, NineAm)));
    }

    [Fact]
    public async Task FR_APT_003_updated_status_is_saved()
    {
        var patientId = await AddPatientAsync("Test Anna Example", "5550101");
        var id = await SeedAsync(Appt(patientId, NineAm));

        await using (var context = fixture.CreateContext())
        {
            var repository = new AppointmentRepository(context);
            var appointment = await repository.GetByIdAsync(id);
            appointment!.Status = "Checked-In";
            await repository.UpdateAsync(appointment);
        }

        await using var reader = fixture.CreateContext();
        Assert.Equal("Checked-In", (await reader.Appointments.SingleAsync()).Status);
    }

    [Fact]
    public async Task FR_APT_003_get_by_id_returns_null_for_an_unknown_appointment()
    {
        await using var context = fixture.CreateContext();

        Assert.Null(await new AppointmentRepository(context).GetByIdAsync(999999));
    }

    private async Task<int> AddPatientAsync(string fullName, string phone)
    {
        await using var context = fixture.CreateContext();
        var patient = new Patient
        {
            FullName = fullName,
            DateOfBirth = new DateOnly(1980, 1, 1),
            Gender = "Female",
            Phone = phone,
            Email = "patient@example.test",
            Address = "Test address"
        };
        context.Patients.Add(patient);
        await context.SaveChangesAsync();
        return patient.Id;
    }

    // Returns the id of the last appointment added.
    private async Task<int> SeedAsync(params Appointment[] appointments)
    {
        await using var context = fixture.CreateContext();
        context.Appointments.AddRange(appointments);
        await context.SaveChangesAsync();
        return appointments[^1].Id;
    }

    private async Task<IReadOnlyList<Appointment>> DailyAsync(DateOnly date)
    {
        await using var context = fixture.CreateContext();
        return await new AppointmentRepository(context).GetDailyAsync(date);
    }

    private async Task<bool> OverlapAsync(DateTime startTime, int? excludeId = null)
    {
        await using var context = fixture.CreateContext();
        return await new AppointmentRepository(context).HasOverlapAsync(startTime, excludeId);
    }

    private static Appointment Appt(int patientId, DateTime startTime, string status = "Scheduled", bool archived = false) => new()
    {
        PatientId = patientId,
        StartTime = startTime,
        DurationMinutes = 30,
        Status = status,
        IsArchived = archived
    };
}
