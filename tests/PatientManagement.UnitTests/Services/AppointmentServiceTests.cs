using NSubstitute;
using PatientManagement.Business.Appointments;
using PatientManagement.Business.DTOs;
using PatientManagement.Business.Services;
using PatientManagement.Data.Abstractions;
using PatientManagement.Data.Entities;

namespace PatientManagement.UnitTests.Services;

// Unit tests for the appointment service. Repositories are substitutes and the clock is fixed.
// Synthetic values only. Test IDs follow CLAUDE.md §4.2 and §10.
public class AppointmentServiceTests
{
    private sealed class FixedClock(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now.ToUniversalTime();

        public override TimeZoneInfo LocalTimeZone => TimeZoneInfo.Utc;
    }

    // Today in the tests is 11 November 2026.
    private static readonly DateTimeOffset Now = new(2026, 11, 11, 8, 0, 0, TimeSpan.Zero);
    private static readonly DateOnly Today = new(2026, 11, 11);
    private static readonly DateTime FutureSlot = new(2026, 11, 12, 9, 0, 0);

    private readonly IAppointmentRepository appointments = Substitute.For<IAppointmentRepository>();
    private readonly IPatientRepository patients = Substitute.For<IPatientRepository>();
    private readonly AppointmentService service;

    public AppointmentServiceTests()
    {
        service = new AppointmentService(appointments, patients, new FixedClock(Now));

        // Mimics the database: a saved appointment receives an identity.
        appointments.AddAsync(Arg.Do<Appointment>(a => a.Id = 41), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);
        appointments.HasOverlapAsync(default, default, default).ReturnsForAnyArgs(false);
        patients.GetByIdAsync(7, Arg.Any<CancellationToken>()).Returns(RegisteredPatient());
    }

    private static Patient RegisteredPatient() => new()
    {
        Id = 7,
        PatientCode = "PT-000007",
        FullName = "Test Anna Example",
        DateOfBirth = new DateOnly(1990, 5, 1),
        Gender = "Female",
        Phone = "5550101",
        Email = "anna@example.test",
        Address = "Test address"
    };

    private static Appointment Booked(DateTime startTime, string status, bool archived = false) => new()
    {
        Id = 1,
        PatientId = 7,
        StartTime = startTime,
        DurationMinutes = 30,
        Status = status,
        IsArchived = archived,
        Patient = RegisteredPatient()
    };

    [Fact]
    public async Task FR_APT_001_schedule_saves_a_scheduled_thirty_minute_appointment()
    {
        var result = await service.ScheduleAsync(new AppointmentCreateDto(7, FutureSlot));

        Assert.True(result.IsSuccess);
        await appointments.Received(1).AddAsync(
            Arg.Is<Appointment>(a =>
                a.PatientId == 7 &&
                a.StartTime == FutureSlot &&
                a.DurationMinutes == 30 &&
                a.Status == AppointmentStatus.Scheduled &&
                !a.IsArchived),
            Arg.Any<CancellationToken>());
        Assert.Equal("PT-000007", result.Value!.PatientCode);
        Assert.Equal(41, result.Value.Id);
    }

    [Fact]
    public async Task FR_APT_001_overlap_is_rejected_and_nothing_is_saved()
    {
        appointments.HasOverlapAsync(FutureSlot, null, Arg.Any<CancellationToken>()).Returns(true);

        var result = await service.ScheduleAsync(new AppointmentCreateDto(7, FutureSlot));

        Assert.Equal([AppointmentService.SlotTakenCode], result.Errors.Select(error => error.Code));
        await appointments.DidNotReceiveWithAnyArgs().AddAsync(default!, default);
    }

    [Fact]
    public async Task FR_APT_001_unregistered_patient_is_rejected()
    {
        var result = await service.ScheduleAsync(new AppointmentCreateDto(999, FutureSlot));

        Assert.Equal([AppointmentService.PatientNotFoundCode], result.Errors.Select(error => error.Code));
        await appointments.DidNotReceiveWithAnyArgs().AddAsync(default!, default);
    }

    [Fact]
    public async Task FR_APT_001_off_grid_time_is_rejected_before_any_lookup()
    {
        var result = await service.ScheduleAsync(new AppointmentCreateDto(7, FutureSlot.AddMinutes(15)));

        Assert.Equal(["Appointment.StartTime.NotOnGrid"], result.Errors.Select(error => error.Code));
        await patients.DidNotReceiveWithAnyArgs().GetByIdAsync(default, default);
        await appointments.DidNotReceiveWithAnyArgs().AddAsync(default!, default);
    }

    [Fact]
    public async Task FR_APT_001_past_date_is_rejected()
    {
        var result = await service.ScheduleAsync(new AppointmentCreateDto(7, Today.AddDays(-1).ToDateTime(new TimeOnly(9, 0))));

        Assert.Equal(["Appointment.Date.InPast"], result.Errors.Select(error => error.Code));
    }

    [Fact]
    public async Task FR_APT_001_slot_taken_between_the_check_and_the_save_is_reported_as_taken()
    {
        appointments.AddAsync(Arg.Any<Appointment>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromException(new SlotTakenException(new InvalidOperationException("duplicate key"))));

        var result = await service.ScheduleAsync(new AppointmentCreateDto(7, FutureSlot));

        Assert.Equal([AppointmentService.SlotTakenCode], result.Errors.Select(error => error.Code));
    }

    [Fact]
    public async Task FR_APT_002_daily_list_returns_the_days_appointments_with_patient_details()
    {
        var first = Booked(FutureSlot, AppointmentStatus.CheckedIn);
        var second = Booked(FutureSlot.AddMinutes(30), AppointmentStatus.Scheduled);
        appointments.GetDailyAsync(DateOnly.FromDateTime(FutureSlot), Arg.Any<CancellationToken>())
            .Returns(new List<Appointment> { first, second });

        var list = await service.GetDailyAsync(DateOnly.FromDateTime(FutureSlot));

        Assert.Equal(
            [FutureSlot, FutureSlot.AddMinutes(30)],
            list.Select(item => item.StartTime));
        Assert.All(list, item =>
        {
            Assert.Equal("Test Anna Example", item.PatientName);
            Assert.Equal("PT-000007", item.PatientCode);
        });
        Assert.Equal(AppointmentStatus.CheckedIn, list[0].Status);
    }

    [Fact]
    public async Task FR_APT_001_slots_mark_only_slot_holding_appointments_as_taken()
    {
        appointments.GetDailyAsync(DateOnly.FromDateTime(FutureSlot), Arg.Any<CancellationToken>())
            .Returns(new List<Appointment>
            {
                Booked(FutureSlot, AppointmentStatus.Scheduled),
                Booked(FutureSlot.AddMinutes(30), AppointmentStatus.Cancelled),
                Booked(FutureSlot.AddMinutes(60), AppointmentStatus.NoShow)
            });

        var slots = await service.GetSlotsAsync(DateOnly.FromDateTime(FutureSlot));

        Assert.Equal(14, slots.Count);
        Assert.True(slots.Single(slot => slot.StartTime == FutureSlot).IsTaken);
        Assert.False(slots.Single(slot => slot.StartTime == FutureSlot.AddMinutes(30)).IsTaken);
        Assert.False(slots.Single(slot => slot.StartTime == FutureSlot.AddMinutes(60)).IsTaken);
    }

    [Fact]
    public async Task FR_APT_003_an_allowed_status_change_is_saved()
    {
        appointments.GetByIdAsync(1, Arg.Any<CancellationToken>())
            .Returns(Booked(FutureSlot, AppointmentStatus.Scheduled));

        var result = await service.UpdateStatusAsync(1, AppointmentStatus.CheckedIn);

        Assert.True(result.IsSuccess);
        Assert.Equal(AppointmentStatus.CheckedIn, result.Value!.Status);
        await appointments.Received(1).UpdateAsync(
            Arg.Is<Appointment>(a => a.Status == AppointmentStatus.CheckedIn),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task FR_APT_003_a_disallowed_status_change_is_rejected_and_not_saved()
    {
        appointments.GetByIdAsync(1, Arg.Any<CancellationToken>())
            .Returns(Booked(FutureSlot, AppointmentStatus.Completed));

        var result = await service.UpdateStatusAsync(1, AppointmentStatus.Scheduled);

        Assert.Equal(["Appointment.Status.NotAllowed"], result.Errors.Select(error => error.Code));
        await appointments.DidNotReceiveWithAnyArgs().UpdateAsync(default!, default);
    }

    [Fact]
    public async Task FR_APT_003_status_change_on_an_unknown_appointment_is_not_found()
    {
        appointments.GetByIdAsync(999, Arg.Any<CancellationToken>()).Returns((Appointment?)null);

        var result = await service.UpdateStatusAsync(999, AppointmentStatus.CheckedIn);

        Assert.Equal([AppointmentService.NotFoundCode], result.Errors.Select(error => error.Code));
    }

    [Fact]
    public async Task FR_APT_003_archive_hides_a_completed_appointment_and_keeps_its_status()
    {
        appointments.GetByIdAsync(1, Arg.Any<CancellationToken>())
            .Returns(Booked(FutureSlot, AppointmentStatus.Completed));

        var result = await service.ArchiveAsync(1);

        Assert.True(result.IsSuccess);
        await appointments.Received(1).UpdateAsync(
            Arg.Is<Appointment>(a => a.IsArchived && a.Status == AppointmentStatus.Completed),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task FR_APT_003_a_scheduled_appointment_cannot_be_archived()
    {
        appointments.GetByIdAsync(1, Arg.Any<CancellationToken>())
            .Returns(Booked(FutureSlot, AppointmentStatus.Scheduled));

        var result = await service.ArchiveAsync(1);

        Assert.Equal(["Appointment.Archive.NotAllowed"], result.Errors.Select(error => error.Code));
        await appointments.DidNotReceiveWithAnyArgs().UpdateAsync(default!, default);
    }

    [Fact]
    public async Task FR_APT_003_an_archived_appointment_is_treated_as_not_found()
    {
        appointments.GetByIdAsync(1, Arg.Any<CancellationToken>())
            .Returns(Booked(FutureSlot, AppointmentStatus.Completed, archived: true));

        var result = await service.UpdateStatusAsync(1, AppointmentStatus.Cancelled);

        Assert.Equal([AppointmentService.NotFoundCode], result.Errors.Select(error => error.Code));
    }
}
