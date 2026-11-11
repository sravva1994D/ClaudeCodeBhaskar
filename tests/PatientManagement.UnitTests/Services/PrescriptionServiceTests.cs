using NSubstitute;
using PatientManagement.Business.Common;
using PatientManagement.Business.DTOs;
using PatientManagement.Business.Services;
using PatientManagement.Data.Abstractions;
using PatientManagement.Data.Entities;

namespace PatientManagement.UnitTests.Services;

// Unit tests for the prescription service: medicine lines, the Print command and the printed lock (FR-RX-001, FR-RX-002).
// Repositories are substitutes and the clock is fixed. Synthetic values only. Test IDs follow CLAUDE.md §4.2 and §10.
public class PrescriptionServiceTests
{
    private sealed class FixedClock(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now.ToUniversalTime();

        public override TimeZoneInfo LocalTimeZone => TimeZoneInfo.Utc;
    }

    private static readonly DateTimeOffset Now = new(2026, 11, 11, 9, 0, 0, TimeSpan.Zero);
    private static readonly DateTime NowLocal = new(2026, 11, 11, 9, 0, 0);

    private readonly IPrescriptionRepository prescriptions = Substitute.For<IPrescriptionRepository>();
    private readonly IConsultationRepository consultations = Substitute.For<IConsultationRepository>();
    private readonly IPatientRepository patients = Substitute.For<IPatientRepository>();
    private readonly IClinicProfileRepository clinicProfiles = Substitute.For<IClinicProfileRepository>();
    private readonly PrescriptionService service;

    public PrescriptionServiceTests()
    {
        service = new PrescriptionService(prescriptions, consultations, patients, clinicProfiles, new FixedClock(Now));
        consultations.GetByIdAsync(Arg.Any<int>(), Arg.Any<bool>(), Arg.Any<CancellationToken>()).Returns(Visit());
        patients.GetByIdAsync(Arg.Any<int>(), Arg.Any<CancellationToken>()).Returns(Patient());
        clinicProfiles.GetAsync(Arg.Any<CancellationToken>()).Returns((ClinicProfile?)null);
        prescriptions.GetByConsultationAsync(Arg.Any<int>(), Arg.Any<CancellationToken>()).Returns((Prescription?)null);
        prescriptions.ReplaceItemsAsync(Arg.Any<int>(), Arg.Any<IReadOnlyList<PrescriptionItem>>(), Arg.Any<CancellationToken>())
            .Returns(true);
        prescriptions.MarkPrintedAsync(Arg.Any<int>(), Arg.Any<DateTime>(), Arg.Any<CancellationToken>()).Returns(true);
    }

    [Fact]
    public async Task FR_RX_001_saving_lines_saves_them_trimmed_in_order_and_leaves_out_blank_ones()
    {
        prescriptions.GetByConsultationAsync(11, Arg.Any<CancellationToken>())
            .Returns((Prescription?)null, Stored(false, "Test Medicine", "Test Second"));

        var result = await service.SaveItemsAsync(11,
        [
            new MedicineLineInput("  Test Medicine  ", null, " daily ", null, null),
            new MedicineLineInput(null, null, null, null, null),
            new MedicineLineInput("Test Second", null, null, null, null)
        ]);

        Assert.True(result.IsSuccess);
        await prescriptions.Received(1).ReplaceItemsAsync(
            Arg.Any<int>(),
            Arg.Is<IReadOnlyList<PrescriptionItem>>(items =>
                items.Select(item => item.Name).SequenceEqual(new[] { "Test Medicine", "Test Second" })
                && items.Select(item => item.SortOrder).SequenceEqual(new short[] { 1, 2 })
                && items[0].Frequency == "daily"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task FR_RX_001_a_lost_race_on_the_first_save_fills_the_existing_prescription()
    {
        prescriptions.AddAsync(Arg.Any<Prescription>(), Arg.Any<CancellationToken>())
            .Returns<Task>(_ => throw new PrescriptionAlreadyExistsException(new Exception("Test race")));
        prescriptions.GetByConsultationAsync(11, Arg.Any<CancellationToken>())
            .Returns((Prescription?)null, Stored(false), Stored(false, "Test Medicine"));

        var result = await service.SaveItemsAsync(11, [new MedicineLineInput("Test Medicine", null, null, null, null)]);

        Assert.True(result.IsSuccess);
        await prescriptions.Received(1).ReplaceItemsAsync(
            Arg.Any<int>(), Arg.Any<IReadOnlyList<PrescriptionItem>>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task FR_RX_001_a_line_without_a_name_saves_nothing()
    {
        var result = await service.SaveItemsAsync(11, [new MedicineLineInput(null, "Test dose", null, null, null)]);

        Assert.False(result.IsSuccess);
        var error = Assert.Single(result.Errors);
        Assert.Equal("Medicine.Name.Required", error.Code);
        Assert.Equal("Lines[0].Name", error.Field);
        await prescriptions.DidNotReceive().AddAsync(Arg.Any<Prescription>(), Arg.Any<CancellationToken>());
        await prescriptions.DidNotReceive().ReplaceItemsAsync(
            Arg.Any<int>(), Arg.Any<IReadOnlyList<PrescriptionItem>>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task FR_RX_001_a_medicine_name_over_200_characters_is_refused()
    {
        var result = await service.SaveItemsAsync(11, [new MedicineLineInput(new string('A', 201), null, null, null, null)]);

        Assert.Equal(["Medicine.Name.TooLong"], result.Errors.Select(error => error.Code));
    }

    [Fact]
    public async Task FR_RX_001_more_than_twenty_medicines_are_refused()
    {
        var lines = Enumerable.Range(1, 21).Select(number => new MedicineLineInput($"Test Medicine {number}", null, null, null, null)).ToList();

        var result = await service.SaveItemsAsync(11, lines);

        Assert.Equal(["Medicine.Lines.TooMany"], result.Errors.Select(error => error.Code));
        await prescriptions.DidNotReceive().ReplaceItemsAsync(
            Arg.Any<int>(), Arg.Any<IReadOnlyList<PrescriptionItem>>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task FR_RX_002_lines_cannot_be_saved_once_printed()
    {
        prescriptions.GetByConsultationAsync(11, Arg.Any<CancellationToken>()).Returns(Stored(true, "Test Medicine"));

        var result = await service.SaveItemsAsync(11, [new MedicineLineInput("Test Late", null, null, null, null)]);

        Assert.Equal([PrescriptionService.PrintedCode], result.Errors.Select(error => error.Code));
        await prescriptions.DidNotReceive().ReplaceItemsAsync(
            Arg.Any<int>(), Arg.Any<IReadOnlyList<PrescriptionItem>>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task FR_RX_002_saving_for_a_missing_visit_is_not_found()
    {
        consultations.GetByIdAsync(Arg.Any<int>(), Arg.Any<bool>(), Arg.Any<CancellationToken>()).Returns((Consultation?)null);

        var result = await service.SaveItemsAsync(99, [new MedicineLineInput("Test Medicine", null, null, null, null)]);

        Assert.Equal([ConsultationService.NotFoundCode], result.Errors.Select(error => error.Code));
    }

    [Fact]
    public async Task FR_RX_002_a_visit_without_medicine_cannot_be_printed()
    {
        var result = await service.MarkPrintedAsync(11);

        Assert.Equal([PrescriptionService.NoMedicineCode], result.Errors.Select(error => error.Code));
        await prescriptions.DidNotReceive().MarkPrintedAsync(Arg.Any<int>(), Arg.Any<DateTime>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task FR_RX_002_printing_marks_the_prescription_and_returns_the_print_view()
    {
        prescriptions.GetByConsultationAsync(11, Arg.Any<CancellationToken>())
            .Returns(Stored(false, "Test Medicine"), Stored(true, "Test Medicine"));

        var result = await service.MarkPrintedAsync(11);

        Assert.True(result.IsSuccess);
        Assert.True(result.Value!.IsPrinted);
        Assert.Equal(NowLocal, result.Value.PrintedAt);
        Assert.Equal("Test Medicine", result.Value.Lines.Single().Name);
        await prescriptions.Received(1).MarkPrintedAsync(3, NowLocal, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task FR_RX_002_a_second_print_changes_nothing()
    {
        prescriptions.GetByConsultationAsync(11, Arg.Any<CancellationToken>()).Returns(Stored(true, "Test Medicine"));

        var result = await service.MarkPrintedAsync(11);

        Assert.True(result.IsSuccess);
        await prescriptions.DidNotReceive().MarkPrintedAsync(Arg.Any<int>(), Arg.Any<DateTime>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task FR_RX_002_a_print_that_loses_its_lines_to_a_removal_is_refused()
    {
        // The lines were removed after the read, so the mark is refused and the prescription is still unprinted.
        prescriptions.GetByConsultationAsync(11, Arg.Any<CancellationToken>())
            .Returns(Stored(false, "Test Medicine"), Stored(false));
        prescriptions.MarkPrintedAsync(Arg.Any<int>(), Arg.Any<DateTime>(), Arg.Any<CancellationToken>()).Returns(false);

        var result = await service.MarkPrintedAsync(11);

        Assert.Equal([PrescriptionService.NoMedicineCode], result.Errors.Select(error => error.Code));
    }

    [Fact]
    public async Task FR_RX_002_print_view_shows_the_visit_the_patient_and_the_clinic_header()
    {
        clinicProfiles.GetAsync(Arg.Any<CancellationToken>()).Returns(new ClinicProfile
        {
            Id = 1,
            ClinicName = "Test Clinic",
            Address = "Test address",
            Phone = "5550199",
            DoctorName = "Test Doctor",
            RegistrationNumber = "TEST-1"
        });
        prescriptions.GetByConsultationAsync(11, Arg.Any<CancellationToken>()).Returns(Stored(false, "Test Medicine"));

        var result = await service.GetPrintModelAsync(11);

        Assert.True(result.IsSuccess);
        var print = result.Value!;
        Assert.True(print.Clinic.IsComplete);
        Assert.Equal("Test Anna Example", print.PatientName);
        Assert.Equal("PT-000007", print.PatientCode);
        Assert.Equal(new DateOnly(1990, 5, 1), print.DateOfBirth);
        Assert.Equal("Female", print.Gender);
        Assert.Equal("Test diagnosis", print.Diagnosis);
        Assert.Equal(120, print.Vitals!.SystolicMmHg);
        Assert.False(print.IsPrinted);
    }

    [Fact]
    public async Task FR_RX_002_an_incomplete_clinic_header_still_prints_and_is_reported()
    {
        clinicProfiles.GetAsync(Arg.Any<CancellationToken>()).Returns(new ClinicProfile { Id = 1, ClinicName = "Test Clinic" });
        prescriptions.GetByConsultationAsync(11, Arg.Any<CancellationToken>())
            .Returns(Stored(false, "Test Medicine"), Stored(true, "Test Medicine"));

        var result = await service.MarkPrintedAsync(11);

        Assert.True(result.IsSuccess);
        Assert.False(result.Value!.Clinic.IsComplete);
    }

    [Fact]
    public async Task FR_RX_001_a_visit_with_no_prescription_has_none_to_show()
    {
        Assert.Null(await service.GetAsync(11));
    }

    [Fact]
    public async Task FR_RX_001_the_saved_medicines_are_returned_in_order()
    {
        prescriptions.GetByConsultationAsync(11, Arg.Any<CancellationToken>()).Returns(Stored(false, "Test First", "Test Second"));

        var result = await service.GetAsync(11);

        Assert.Equal(["Test First", "Test Second"], result!.Lines.Select(line => line.Name));
    }

    private static Consultation Visit() => new()
    {
        Id = 11,
        PatientId = 7,
        VisitDateTime = NowLocal,
        Diagnosis = "Test diagnosis",
        Vitals = new VitalSign { ConsultationId = 11, TemperatureC = 36.8m, SystolicMmHg = 120, DiastolicMmHg = 80, PulseBpm = 72 }
    };

    private static Patient Patient() => new()
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

    // A stored prescription (Id 3) with the given medicine names, printed or not.
    private static Prescription Stored(bool printed, params string[] names) => new()
    {
        Id = 3,
        ConsultationId = 11,
        IsPrinted = printed,
        PrintedAt = printed ? NowLocal : null,
        Items = names.Select((name, index) => new PrescriptionItem
        {
            Id = index + 1,
            PrescriptionId = 3,
            Name = name,
            SortOrder = (short)(index + 1)
        }).ToList()
    };
}
