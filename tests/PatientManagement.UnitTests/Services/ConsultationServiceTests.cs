using NSubstitute;
using PatientManagement.Business.Appointments;
using PatientManagement.Business.DTOs;
using PatientManagement.Business.Services;
using PatientManagement.Business.Validators;
using PatientManagement.Data.Abstractions;
using PatientManagement.Data.Entities;

namespace PatientManagement.UnitTests.Services;

// Unit tests for the consultation service. Repositories are substitutes and the clock is fixed.
// Synthetic values only. Test IDs follow CLAUDE.md §4.2 and §10.
public class ConsultationServiceTests
{
    private sealed class FixedClock(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now.ToUniversalTime();

        public override TimeZoneInfo LocalTimeZone => TimeZoneInfo.Utc;
    }

    private static readonly DateTimeOffset Now = new(2026, 11, 11, 9, 0, 0, TimeSpan.Zero);
    private static readonly DateTime NowLocal = new(2026, 11, 11, 9, 0, 0);

    private readonly IConsultationRepository consultations = Substitute.For<IConsultationRepository>();
    private readonly IAppointmentRepository appointments = Substitute.For<IAppointmentRepository>();
    private readonly IPatientRepository patients = Substitute.For<IPatientRepository>();
    private readonly IPrescriptionRepository prescriptions = Substitute.For<IPrescriptionRepository>();
    private readonly ConsultationService service;

    public ConsultationServiceTests()
    {
        service = new ConsultationService(consultations, prescriptions, appointments, patients, new FixedClock(Now));
        prescriptions.GetByConsultationAsync(Arg.Any<int>(), Arg.Any<CancellationToken>()).Returns((Prescription?)null);
        consultations.UpdateAsync(Arg.Any<Consultation>(), Arg.Any<CancellationToken>()).Returns(true);
        patients.GetByIdAsync(7, Arg.Any<CancellationToken>()).Returns(Patient());
        consultations.MarkFinishedAsync(
            Arg.Any<int>(), Arg.Any<DateTime>(), Arg.Any<Appointment?>(), Arg.Any<CancellationToken>()).Returns(true);
    }

    private static Patient Patient(int id = 7) => new()
    {
        Id = id,
        PatientCode = $"PT-{id:D6}",
        FullName = "Test Anna Example",
        DateOfBirth = new DateOnly(1990, 5, 1),
        Gender = "Female",
        Phone = "5550101",
        Email = "anna@example.test",
        Address = "Test address"
    };

    private static Appointment Booked(string status, int patientId = 7, bool archived = false) => new()
    {
        Id = 5,
        PatientId = patientId,
        StartTime = NowLocal,
        DurationMinutes = 30,
        Status = status,
        IsArchived = archived
    };

    private static Consultation Visit(int? appointmentId = null, DateTime? finishedAt = null, VitalSign? vitals = null) => new()
    {
        Id = 11,
        PatientId = 7,
        AppointmentId = appointmentId,
        VisitDateTime = NowLocal,
        FinishedAt = finishedAt,
        Vitals = vitals
    };

    private static VitalSign SavedVitals() => new()
    {
        ConsultationId = 11,
        TemperatureC = 36.8m,
        SystolicMmHg = 120,
        DiastolicMmHg = 80,
        PulseBpm = 72
    };

    private static VitalsInput Normal() => new(36.8m, 120, 80, 72);

    private static VitalsInput Empty() => new(null, null, null, null);

    private static ConsultationSaveInput Save(VitalsInput vitalsInput, string? complaints = "  Test cough  ", string? diagnosis = "Test diagnosis") =>
        new(complaints, diagnosis, vitalsInput);

    [Fact]
    public async Task FR_HIS_001_walk_in_start_creates_a_visit_at_the_current_time_with_no_appointment()
    {
        var result = await service.StartAsync(7, appointmentId: null);

        Assert.True(result.IsSuccess);
        await consultations.Received(1).AddAsync(
            Arg.Is<Consultation>(c => c.PatientId == 7 && c.AppointmentId == null && c.VisitDateTime == NowLocal),
            null,
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task FR_APT_003_start_from_a_checked_in_appointment_sets_it_to_in_consultation()
    {
        appointments.GetByIdAsync(5, Arg.Any<CancellationToken>()).Returns(Booked(AppointmentStatus.CheckedIn));

        var result = await service.StartAsync(7, appointmentId: 5);

        Assert.True(result.IsSuccess);
        await consultations.Received(1).AddAsync(
            Arg.Is<Consultation>(c => c.AppointmentId == 5 && c.VisitDateTime == NowLocal),
            Arg.Is<Appointment>(a => a.Status == AppointmentStatus.InConsultation),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task FR_APT_003_start_from_a_scheduled_appointment_is_refused_and_names_the_status()
    {
        appointments.GetByIdAsync(5, Arg.Any<CancellationToken>()).Returns(Booked(AppointmentStatus.Scheduled));

        var result = await service.StartAsync(7, appointmentId: 5);

        var error = Assert.Single(result.Errors);
        Assert.Equal(ConsultationService.AppointmentNotCheckedInCode, error.Code);
        Assert.Contains("Scheduled", error.Message);
        await consultations.DidNotReceiveWithAnyArgs().AddAsync(default!, default, default);
    }

    [Fact]
    public async Task FR_HIS_001_start_with_another_patients_appointment_is_refused()
    {
        appointments.GetByIdAsync(5, Arg.Any<CancellationToken>()).Returns(Booked(AppointmentStatus.CheckedIn, patientId: 99));

        var result = await service.StartAsync(7, appointmentId: 5);

        Assert.Equal([ConsultationService.AppointmentOtherPatientCode], result.Errors.Select(e => e.Code));
        await consultations.DidNotReceiveWithAnyArgs().AddAsync(default!, default, default);
    }

    [Fact]
    public async Task FR_HIS_001_start_with_an_archived_or_unknown_appointment_is_not_found()
    {
        appointments.GetByIdAsync(5, Arg.Any<CancellationToken>()).Returns(Booked(AppointmentStatus.Completed, archived: true));
        appointments.GetByIdAsync(6, Arg.Any<CancellationToken>()).Returns((Appointment?)null);

        Assert.Equal([ConsultationService.AppointmentNotFoundCode], (await service.StartAsync(7, 5)).Errors.Select(e => e.Code));
        Assert.Equal([ConsultationService.AppointmentNotFoundCode], (await service.StartAsync(7, 6)).Errors.Select(e => e.Code));
    }

    [Fact]
    public async Task FR_HIS_001_a_second_start_for_the_same_appointment_is_refused()
    {
        appointments.GetByIdAsync(5, Arg.Any<CancellationToken>()).Returns(Booked(AppointmentStatus.CheckedIn));
        consultations.GetByAppointmentIdAsync(5, Arg.Any<CancellationToken>()).Returns(Visit(appointmentId: 5));

        var result = await service.StartAsync(7, appointmentId: 5);

        Assert.Equal([ConsultationService.AppointmentAlreadyVisitedCode], result.Errors.Select(e => e.Code));
        await consultations.DidNotReceiveWithAnyArgs().AddAsync(default!, default, default);
    }

    [Fact]
    public async Task FR_HIS_001_a_start_that_loses_a_race_to_a_second_visit_is_refused()
    {
        appointments.GetByIdAsync(5, Arg.Any<CancellationToken>()).Returns(Booked(AppointmentStatus.CheckedIn));
        consultations.AddAsync(default!, default, default)
            .ReturnsForAnyArgs(Task.FromException(new AppointmentAlreadyVisitedException(new InvalidOperationException("duplicate key"))));

        var result = await service.StartAsync(7, appointmentId: 5);

        Assert.Equal([ConsultationService.AppointmentAlreadyVisitedCode], result.Errors.Select(e => e.Code));
    }

    [Fact]
    public async Task FR_HIS_001_start_for_an_unknown_patient_is_refused()
    {
        var result = await service.StartAsync(99, appointmentId: null);

        Assert.Equal([ConsultationService.PatientNotFoundCode], result.Errors.Select(e => e.Code));
    }

    [Fact]
    public async Task FR_CMP_001_FR_DIA_001_save_stores_complaints_and_diagnosis_trimmed()
    {
        consultations.GetByIdAsync(11, true, Arg.Any<CancellationToken>()).Returns(Visit());

        var result = await service.SaveAsync(11, Save(Empty()), confirmWarnings: false);

        Assert.True(result.IsSuccess);
        await consultations.Received(1).UpdateAsync(
            Arg.Is<Consultation>(c => c.Complaints == "Test cough" && c.Diagnosis == "Test diagnosis"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task FR_CMP_001_blank_complaints_are_stored_as_no_text()
    {
        consultations.GetByIdAsync(11, true, Arg.Any<CancellationToken>()).Returns(Visit());

        await service.SaveAsync(11, Save(Empty(), complaints: "   ", diagnosis: null), confirmWarnings: false);

        await consultations.Received(1).UpdateAsync(
            Arg.Is<Consultation>(c => c.Complaints == null && c.Diagnosis == null),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task FR_RX_002_a_visit_with_a_printed_prescription_cannot_be_saved()
    {
        consultations.GetByIdAsync(11, true, Arg.Any<CancellationToken>()).Returns(Visit());
        prescriptions.GetByConsultationAsync(11, Arg.Any<CancellationToken>()).Returns(new Prescription
        {
            Id = 3,
            ConsultationId = 11,
            IsPrinted = true,
            PrintedAt = NowLocal
        });

        var result = await service.SaveAsync(11, Save(new VitalsInput(36.8m, 120, 80, 72)), confirmWarnings: true);

        Assert.False(result.IsSuccess);
        Assert.Equal([PrescriptionService.PrintedCode], result.Errors.Select(e => e.Code));
        await consultations.DidNotReceive().UpdateAsync(Arg.Any<Consultation>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task FR_VIT_001_out_of_range_vitals_ask_for_confirmation_and_save_nothing()
    {
        consultations.GetByIdAsync(11, true, Arg.Any<CancellationToken>()).Returns(Visit());

        var result = await service.SaveAsync(11, Save(new VitalsInput(43.0m, 120, 80, 72)), confirmWarnings: false);

        Assert.True(result.RequiresConfirmation);
        Assert.Contains("43.0 °C", result.Warnings.Single().Message);
        await consultations.DidNotReceiveWithAnyArgs().UpdateAsync(default!, default);
    }

    [Fact]
    public async Task FR_VIT_001_confirmed_out_of_range_vitals_are_saved()
    {
        consultations.GetByIdAsync(11, true, Arg.Any<CancellationToken>()).Returns(Visit());

        var result = await service.SaveAsync(11, Save(new VitalsInput(43.0m, 120, 80, 72)), confirmWarnings: true);

        Assert.True(result.IsSuccess);
        Assert.Single(result.Warnings);
        await consultations.Received(1).UpdateAsync(Arg.Is<Consultation>(c => c.Vitals != null && (c.Vitals!.TemperatureC == 43.0m && c.Vitals!.ConsultationId == 11)), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task FR_VIT_001_saving_without_vitals_gives_a_notice_and_needs_no_confirmation()
    {
        consultations.GetByIdAsync(11, true, Arg.Any<CancellationToken>()).Returns(Visit());

        var result = await service.SaveAsync(11, Save(Empty()), confirmWarnings: false);

        Assert.True(result.IsSuccess);
        Assert.Equal([VitalsValidator.Notices(Empty()).Single().Code], result.Warnings.Select(w => w.Code));
    }

    [Fact]
    public async Task FR_VIT_001_partial_vitals_are_refused_and_nothing_is_saved()
    {
        consultations.GetByIdAsync(11, true, Arg.Any<CancellationToken>()).Returns(Visit());

        var result = await service.SaveAsync(11, Save(new VitalsInput(36.8m, 120, null, 72)), confirmWarnings: true);

        Assert.Equal(["Vitals.Incomplete"], result.Errors.Select(e => e.Code));
        await consultations.DidNotReceiveWithAnyArgs().UpdateAsync(default!, default);
    }

    [Fact]
    public async Task FR_VIT_001_vitals_can_be_added_on_a_later_save()
    {
        consultations.GetByIdAsync(11, true, Arg.Any<CancellationToken>()).Returns(Visit(vitals: null));

        var result = await service.SaveAsync(11, Save(Normal()), confirmWarnings: false);

        Assert.True(result.IsSuccess);
        await consultations.Received(1).UpdateAsync(Arg.Is<Consultation>(c => c.Vitals != null && (c.Vitals!.ConsultationId == 11 && c.Vitals!.SystolicMmHg == 120 && c.Vitals!.PulseBpm == 72)), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task FR_VIT_001_saving_without_vitals_leaves_saved_vitals_unchanged()
    {
        consultations.GetByIdAsync(11, true, Arg.Any<CancellationToken>()).Returns(Visit(vitals: SavedVitals()));

        await service.SaveAsync(11, Save(Empty()), confirmWarnings: false);

        await consultations.Received(1).UpdateAsync(
            Arg.Is<Consultation>(c => c.Vitals!.TemperatureC == 36.8m && c.Vitals.SystolicMmHg == 120),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task FR_APT_003_finish_of_an_appointment_visit_completes_the_appointment()
    {
        consultations.GetByIdAsync(11, true, Arg.Any<CancellationToken>()).Returns(Visit(appointmentId: 5));
        appointments.GetByIdAsync(5, Arg.Any<CancellationToken>()).Returns(Booked(AppointmentStatus.InConsultation));

        var result = await service.FinishAsync(11);

        Assert.True(result.IsSuccess);
        await consultations.Received(1).MarkFinishedAsync(
            11, NowLocal,
            Arg.Is<Appointment>(a => a.Status == AppointmentStatus.Completed),
            Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData(AppointmentStatus.CheckedIn)]
    [InlineData(AppointmentStatus.Scheduled)]
    public async Task FR_APT_003_finish_from_a_status_other_than_in_consultation_is_refused(string status)
    {
        consultations.GetByIdAsync(11, true, Arg.Any<CancellationToken>()).Returns(Visit(appointmentId: 5));
        appointments.GetByIdAsync(5, Arg.Any<CancellationToken>()).Returns(Booked(status));

        var result = await service.FinishAsync(11);

        var error = Assert.Single(result.Errors);
        Assert.Equal(ConsultationService.FinishStatusCode, error.Code);
        Assert.Contains(status, error.Message);
        await consultations.DidNotReceiveWithAnyArgs().MarkFinishedAsync(default, default, default, default);
    }

    [Fact]
    public async Task FR_APT_003_finish_twice_is_refused()
    {
        consultations.GetByIdAsync(11, true, Arg.Any<CancellationToken>()).Returns(Visit(finishedAt: NowLocal));

        var result = await service.FinishAsync(11);

        Assert.Equal([ConsultationService.AlreadyFinishedCode], result.Errors.Select(e => e.Code));
    }

    [Fact]
    public async Task FR_APT_003_finish_that_loses_a_race_to_another_finish_is_refused()
    {
        consultations.GetByIdAsync(11, true, Arg.Any<CancellationToken>()).Returns(Visit());
        consultations.MarkFinishedAsync(
            11, Arg.Any<DateTime>(), null, Arg.Any<CancellationToken>()).Returns(false);

        var result = await service.FinishAsync(11);

        Assert.Equal([ConsultationService.AlreadyFinishedCode], result.Errors.Select(e => e.Code));
    }

    [Fact]
    public async Task FR_HIS_002_visit_whose_patient_is_missing_is_not_found_and_nothing_is_saved()
    {
        consultations.GetByIdAsync(11, true, Arg.Any<CancellationToken>()).Returns(Visit());
        patients.GetByIdAsync(7, Arg.Any<CancellationToken>()).Returns((Patient?)null);

        var read = await service.GetAsync(11);
        var saved = await service.SaveAsync(11, Save(Empty()), confirmWarnings: false);
        var finished = await service.FinishAsync(11);

        Assert.Equal(ConsultationService.NotFoundCode, read.Errors.Single().Code);
        Assert.Equal(ConsultationService.NotFoundCode, saved.Errors.Single().Code);
        Assert.Equal(ConsultationService.NotFoundCode, finished.Errors.Single().Code);
        await consultations.DidNotReceiveWithAnyArgs().UpdateAsync(default!, default);
        await consultations.DidNotReceiveWithAnyArgs().MarkFinishedAsync(default, default, default, default);
    }

    [Fact]
    public async Task FR_HIS_001_finish_of_a_walk_in_sets_the_finished_time_and_changes_no_appointment()
    {
        consultations.GetByIdAsync(11, true, Arg.Any<CancellationToken>()).Returns(Visit(appointmentId: null));

        var result = await service.FinishAsync(11);

        Assert.True(result.IsSuccess);
        await consultations.Received(1).MarkFinishedAsync(
            11, NowLocal, null, Arg.Any<CancellationToken>());
        await appointments.DidNotReceiveWithAnyArgs().GetByIdAsync(default, default);
    }

    [Fact]
    public async Task FR_HIS_002_detail_shows_the_vitals_complaints_and_diagnosis()
    {
        var visit = Visit(vitals: SavedVitals());
        visit.Complaints = "Test cough";
        visit.Diagnosis = "Test diagnosis";
        consultations.GetByIdAsync(11, true, Arg.Any<CancellationToken>()).Returns(visit);

        var result = await service.GetAsync(11);

        Assert.True(result.IsSuccess);
        Assert.Equal("Test cough", result.Value!.Complaints);
        Assert.Equal("Test diagnosis", result.Value.Diagnosis);
        Assert.Equal(120, result.Value.Vitals!.SystolicMmHg);
        Assert.Equal("Test Anna Example", result.Value.PatientName);
    }

    [Fact]
    public async Task FR_HIS_002_unknown_visit_is_not_found()
    {
        consultations.GetByIdAsync(999, true, Arg.Any<CancellationToken>()).Returns((Consultation?)null);

        var result = await service.GetAsync(999);

        Assert.Equal([ConsultationService.NotFoundCode], result.Errors.Select(e => e.Code));
    }

    [Fact]
    public async Task FR_HIS_001_visit_list_maps_each_visit_with_its_finished_state()
    {
        consultations.ListByPatientAsync(7, Arg.Any<CancellationToken>()).Returns(new List<Consultation>
        {
            new() { Id = 2, PatientId = 7, VisitDateTime = NowLocal.AddDays(1), FinishedAt = NowLocal.AddDays(1) },
            new() { Id = 1, PatientId = 7, VisitDateTime = NowLocal }
        });

        var list = await service.ListVisitsAsync(7);

        Assert.Equal([2, 1], list.Select(v => v.Id));
        Assert.Equal([true, false], list.Select(v => v.IsFinished));
    }
}
