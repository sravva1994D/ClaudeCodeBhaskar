using NSubstitute;
using PatientManagement.Business.DTOs;
using PatientManagement.Business.Services;
using PatientManagement.Data.Abstractions;
using PatientManagement.Data.Entities;

namespace PatientManagement.UnitTests.Services;

// Unit tests for the patient service. The repository is a substitute and the clock is fixed.
// Synthetic values only. Test IDs follow CLAUDE.md §4.2 and §10.
public class PatientServiceTests
{
    private sealed class FixedClock(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now.ToUniversalTime();

        public override TimeZoneInfo LocalTimeZone => TimeZoneInfo.Utc;
    }

    private static readonly DateTimeOffset Now = new(2026, 11, 11, 9, 0, 0, TimeSpan.Zero);

    private readonly IPatientRepository patients = Substitute.For<IPatientRepository>();
    private readonly IConsultationRepository consultations = Substitute.For<IConsultationRepository>();
    private readonly PatientService service;

    public PatientServiceTests()
    {
        var clock = new FixedClock(Now);
        service = new PatientService(patients, new AgeCalculator(clock), consultations, clock);

        // Mimics the database: a saved patient receives an identity and a computed code.
        patients.AddAsync(Arg.Do<Patient>(p => { p.Id = 7; p.PatientCode = "PT-000007"; }), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);
        patients.FindPossibleDuplicatesAsync(default!, default, default!, default, default)
            .ReturnsForAnyArgs(new List<Patient>());
    }

    private static PatientInput ValidInput(string fullName = "Test Anna Example") => new(
        FullName: fullName,
        DateOfBirth: new DateOnly(1990, 5, 1),
        Gender: "Female",
        Phone: "5550101",
        Email: "anna@example.test",
        Address: "Test address");

    private static Patient ExistingPatient() => new()
    {
        Id = 123,
        PatientCode = "PT-000123",
        FullName = "Test Anna Example",
        DateOfBirth = new DateOnly(1990, 5, 1),
        Gender = "Female",
        Phone = "5550101",
        Email = "anna@example.test",
        Address = "Test address"
    };

    [Fact]
    public async Task FR_PAT_001_register_saves_a_valid_patient_and_returns_its_profile()
    {
        var result = await service.RegisterAsync(ValidInput(), confirmWarnings: false);

        Assert.True(result.IsSuccess);
        Assert.Equal("PT-000007", result.Value!.PatientCode);
        await patients.Received(1).AddAsync(Arg.Is<Patient>(p => p.FullName == "Test Anna Example"), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task FR_PAT_001_register_with_a_missing_field_returns_errors_and_saves_nothing()
    {
        var result = await service.RegisterAsync(ValidInput() with { Address = null }, confirmWarnings: false);

        Assert.False(result.IsSuccess);
        Assert.Equal(["Patient.Address.Required"], result.Errors.Select(e => e.Code));
        await patients.DidNotReceiveWithAnyArgs().AddAsync(default!, default);
    }

    [Fact]
    public async Task FR_PAT_001_register_returns_the_age_calculated_from_the_date_of_birth()
    {
        var result = await service.RegisterAsync(ValidInput(), confirmWarnings: false);

        Assert.Equal(36, result.Value!.Age);
    }

    [Fact]
    public async Task FR_PAT_001_possible_duplicate_asks_for_confirmation_and_saves_nothing()
    {
        patients.FindPossibleDuplicatesAsync(default!, default, default!, default, default)
            .ReturnsForAnyArgs(new List<Patient> { ExistingPatient() });

        var result = await service.RegisterAsync(ValidInput(), confirmWarnings: false);

        Assert.True(result.RequiresConfirmation);
        Assert.Equal(PatientService.DuplicateCode, result.Warnings.Single().Code);
        Assert.Contains("PT-000123", result.Warnings.Single().Message);
        await patients.DidNotReceiveWithAnyArgs().AddAsync(default!, default);
    }

    [Fact]
    public async Task FR_PAT_001_possible_duplicate_is_saved_after_confirmation()
    {
        patients.FindPossibleDuplicatesAsync(default!, default, default!, default, default)
            .ReturnsForAnyArgs(new List<Patient> { ExistingPatient() });

        var result = await service.RegisterAsync(ValidInput(), confirmWarnings: true);

        Assert.True(result.IsSuccess);
        await patients.Received(1).AddAsync(Arg.Any<Patient>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task FR_PAT_002_duplicate_check_uses_the_trimmed_name_and_phone()
    {
        await service.RegisterAsync(ValidInput("  Test Anna Example  ") with { Phone = " 5550101 " }, confirmWarnings: false);

        await patients.Received(1).FindPossibleDuplicatesAsync(
            "Test Anna Example", new DateOnly(1990, 5, 1), "5550101", null, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task FR_PAT_002_edit_saves_the_changes_and_keeps_the_patient_code()
    {
        var patient = ExistingPatient();
        patients.GetByIdAsync(123, Arg.Any<CancellationToken>()).Returns(patient);

        var result = await service.UpdateAsync(123, ValidInput("Test Anna Changed"), confirmWarnings: false);

        Assert.True(result.IsSuccess);
        Assert.Equal("Test Anna Changed", patient.FullName);
        Assert.Equal("PT-000123", patient.PatientCode);
        await patients.Received(1).UpdateAsync(patient, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task FR_PAT_002_edit_excludes_the_same_patient_from_the_duplicate_check()
    {
        patients.GetByIdAsync(123, Arg.Any<CancellationToken>()).Returns(ExistingPatient());

        await service.UpdateAsync(123, ValidInput(), confirmWarnings: false);

        await patients.Received(1).FindPossibleDuplicatesAsync(
            "Test Anna Example", new DateOnly(1990, 5, 1), "5550101", 123, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task FR_PAT_002_edit_of_an_unknown_patient_is_not_found()
    {
        patients.GetByIdAsync(999, Arg.Any<CancellationToken>()).Returns((Patient?)null);

        var result = await service.UpdateAsync(999, ValidInput(), confirmWarnings: false);

        Assert.False(result.IsSuccess);
        Assert.Equal(PatientService.NotFoundCode, result.Errors.Single().Code);
    }

    [Fact]
    public async Task FR_PAT_002_invalid_edit_saves_nothing()
    {
        var patient = ExistingPatient();
        patients.GetByIdAsync(123, Arg.Any<CancellationToken>()).Returns(patient);

        var result = await service.UpdateAsync(123, ValidInput() with { Email = "not-an-email" }, confirmWarnings: false);

        Assert.False(result.IsSuccess);
        await patients.DidNotReceiveWithAnyArgs().UpdateAsync(default!, default);
    }

    [Fact]
    public async Task FR_PAT_003_profile_shows_every_field_and_the_age()
    {
        patients.GetByIdAsync(123, Arg.Any<CancellationToken>()).Returns(ExistingPatient());

        var result = await service.GetAsync(123);

        Assert.True(result.IsSuccess);
        Assert.Equal("PT-000123", result.Value!.PatientCode);
        Assert.Equal("Test Anna Example", result.Value.FullName);
        Assert.Equal("anna@example.test", result.Value.Email);
        Assert.Equal(36, result.Value.Age);
    }

    [Fact]
    public async Task FR_HIS_001_profile_shows_the_latest_visit_time_and_none_when_there_is_no_visit()
    {
        patients.GetByIdAsync(123, Arg.Any<CancellationToken>()).Returns(ExistingPatient());
        var latest = new DateTime(2026, 11, 10, 9, 30, 0);
        consultations.GetLatestVisitTimeAsync(123, Arg.Any<CancellationToken>()).Returns(latest);

        var withVisit = await service.GetAsync(123);

        consultations.GetLatestVisitTimeAsync(123, Arg.Any<CancellationToken>()).Returns((DateTime?)null);
        var withoutVisit = await service.GetAsync(123);

        Assert.Equal(latest, withVisit.Value!.LastVisit);
        Assert.Null(withoutVisit.Value!.LastVisit);
    }

    [Fact]
    public async Task FR_PAT_003_profile_of_an_unknown_patient_is_not_found()
    {
        patients.GetByIdAsync(999, Arg.Any<CancellationToken>()).Returns((Patient?)null);

        var result = await service.GetAsync(999);

        Assert.Equal(PatientService.NotFoundCode, result.Errors.Single().Code);
    }

    [Fact]
    public async Task FR_PAT_004_search_by_name_returns_the_matching_rows()
    {
        patients.SearchAsync("Anna", Arg.Any<CancellationToken>()).Returns(new List<Patient> { ExistingPatient() });

        var result = await service.SearchAsync("Anna");

        Assert.True(result.IsSuccess);
        Assert.Equal(["PT-000123"], result.Value!.Select(row => row.PatientCode));
    }

    [Fact]
    public async Task FR_PAT_004_search_by_phone_returns_the_matching_rows()
    {
        patients.SearchAsync("0101", Arg.Any<CancellationToken>()).Returns(new List<Patient> { ExistingPatient() });

        var result = await service.SearchAsync("0101");

        Assert.Single(result.Value!);
    }

    [Fact]
    public async Task FR_PAT_004_search_with_no_match_is_a_success_with_no_rows()
    {
        patients.SearchAsync("Zara", Arg.Any<CancellationToken>()).Returns(new List<Patient>());

        var result = await service.SearchAsync("Zara");

        Assert.True(result.IsSuccess);
        Assert.Empty(result.Value!);
    }

    [Fact]
    public async Task FR_PAT_004_empty_search_term_is_an_error_and_is_not_searched()
    {
        var result = await service.SearchAsync("   ");

        Assert.False(result.IsSuccess);
        Assert.Equal("Search.Empty", result.Errors.Single().Code);
        await patients.DidNotReceiveWithAnyArgs().SearchAsync(default!, default);
    }

    [Fact]
    public async Task FR_PAT_004_search_term_is_trimmed_before_it_is_searched()
    {
        patients.SearchAsync("Anna", Arg.Any<CancellationToken>()).Returns(new List<Patient>());

        await service.SearchAsync("  Anna  ");

        await patients.Received(1).SearchAsync("Anna", Arg.Any<CancellationToken>());
    }
}
