using System.Text;
using NSubstitute;
using PatientManagement.Business.Common;
using PatientManagement.Business.Services;
using PatientManagement.Data.Abstractions;
using PatientManagement.Data.Entities;


namespace PatientManagement.UnitTests.Services;

// Unit tests for the export service and the CSV layout (FR-EXP-001, FR-EXP-002). Repositories are substitutes.
// Synthetic values only. Test IDs follow CLAUDE.md §4.2 and §10.
public class ExportServiceTests
{
    private const string Header =
        "PatientCode,FullName,DateOfBirth,Gender,Phone,Email,Address,VisitDate,VisitTime,TemperatureC,SystolicMmHg,DiastolicMmHg,PulseBpm,Complaints,Diagnosis,Medicines";

    private static readonly DateTime Morning = new(2026, 11, 3, 14, 30, 0);

    private readonly IPatientRepository patients = Substitute.For<IPatientRepository>();
    private readonly IConsultationRepository consultations = Substitute.For<IConsultationRepository>();
    private readonly IClinicProfileRepository clinics = Substitute.For<IClinicProfileRepository>();
    private readonly ExportService service;

    public ExportServiceTests()
    {
        service = new ExportService(patients, consultations, clinics, new CsvWriter());
        patients.GetByIdAsync(7, Arg.Any<CancellationToken>()).Returns(Patient());
        clinics.GetAsync(Arg.Any<CancellationToken>()).Returns((ClinicProfile?)null);
        GivenVisits();
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

    private static ExportVisitRow Visit(DateTime visitTime, string? complaints = "Test cough", string? diagnosis = "Test viral fever",
        decimal? temperature = 36.8m, short? systolic = 120, short? diastolic = 80, short? pulse = 72,
        params ExportMedicine[] medicines) =>
        new(1, visitTime, complaints, diagnosis, temperature, systolic, diastolic, pulse, medicines);

    private void GivenPhone(string phone)
    {
        var patient = Patient();
        patient.Phone = phone;
        patients.GetByIdAsync(7, Arg.Any<CancellationToken>()).Returns(patient);
    }

    private void GivenVisits(params ExportVisitRow[] rows) =>
        consultations.ListForExportAsync(Arg.Any<int>(), Arg.Any<DateOnly?>(), Arg.Any<DateOnly?>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<IReadOnlyList<ExportVisitRow>>(rows));

    // The file text without its byte-order mark.
    private static string TextOf(byte[] content) => Encoding.UTF8.GetString(content).TrimStart('﻿');

    [Fact]
    public async Task FR_EXP_001_csv_has_the_sixteen_columns_in_order()
    {
        GivenVisits(Visit(Morning));

        var file = (await service.BuildCsvAsync(7)).Value!;

        Assert.StartsWith(Header + "\r\n", TextOf(file.Content));
    }

    [Fact]
    public async Task FR_EXP_001_csv_starts_with_a_byte_order_mark_and_uses_crlf_line_ends()
    {
        GivenVisits(Visit(Morning));

        var file = (await service.BuildCsvAsync(7)).Value!;

        Assert.Equal(new byte[] { 0xEF, 0xBB, 0xBF }, file.Content[..3]);
        Assert.EndsWith("\r\n", TextOf(file.Content));
    }

    [Fact]
    public async Task FR_EXP_001_csv_row_repeats_the_patient_and_uses_iso_dates()
    {
        GivenVisits(Visit(Morning, medicines: new ExportMedicine("Test Medicine", "500 mg", "Twice daily", "5 days", "After food")));

        var file = (await service.BuildCsvAsync(7)).Value!;

        Assert.Contains(
            "PT-000007,Test Anna Example,1990-05-01,Female,5550101,anna@example.test,Test address,2026-11-03,14:30,36.8,120,80,72,Test cough,Test viral fever,Test Medicine; 500 mg; Twice daily; 5 days; After food",
            TextOf(file.Content));
    }

    [Fact]
    public async Task FR_EXP_001_csv_lists_visits_oldest_first()
    {
        GivenVisits(Visit(Morning.AddDays(-2), complaints: "Test older"), Visit(Morning, complaints: "Test newer"));

        var text = TextOf((await service.BuildCsvAsync(7)).Value!.Content);

        Assert.True(text.IndexOf("Test older", StringComparison.Ordinal) < text.IndexOf("Test newer", StringComparison.Ordinal));
    }

    [Fact]
    public async Task FR_EXP_001_csv_leaves_vitals_blank_when_not_recorded()
    {
        GivenVisits(Visit(Morning, complaints: null, diagnosis: null, temperature: null, systolic: null, diastolic: null, pulse: null));

        var line = TextOf((await service.BuildCsvAsync(7)).Value!.Content).Split("\r\n")[1];

        Assert.EndsWith(",2026-11-03,14:30,,,,,,,", line);
    }

    [Fact]
    public async Task FR_EXP_001_csv_medicines_form_one_cell_with_blank_parts_omitted()
    {
        GivenVisits(Visit(Morning,
            medicines:
            [
                new ExportMedicine("Test Medicine A", "250 mg", null, "3 days", null),
                new ExportMedicine("Test Medicine B", null, null, null, null)
            ]));

        var text = TextOf((await service.BuildCsvAsync(7)).Value!.Content);

        Assert.Contains("Test Medicine A; 250 mg; 3 days | Test Medicine B", text);
    }

    [Fact]
    public async Task FR_EXP_001_csv_quotes_commas_quotes_and_line_breaks()
    {
        GivenVisits(Visit(Morning, complaints: "Test line one\r\nTest \"quoted\", line two", diagnosis: null));

        var text = TextOf((await service.BuildCsvAsync(7)).Value!.Content);

        Assert.Contains("\"Test line one\r\nTest \"\"quoted\"\", line two\"", text);
    }

    [Fact]
    public async Task FR_EXP_001_csv_text_that_starts_with_a_formula_character_is_shown_as_text()
    {
        GivenVisits(Visit(Morning, complaints: "=1+1", diagnosis: "-Test sign"));

        var text = TextOf((await service.BuildCsvAsync(7)).Value!.Content);

        Assert.Contains(",'=1+1,'-Test sign,", text);
    }

    [Theory]
    [InlineData("+91 98765 43210")]
    [InlineData("+1 (555) 010-1234")]
    public async Task FR_EXP_001_phone_number_with_a_leading_plus_is_not_changed(string phone)
    {
        GivenPhone(phone);
        GivenVisits(Visit(Morning));

        var text = TextOf((await service.BuildCsvAsync(7)).Value!.Content);

        Assert.Contains($",{phone},anna@example.test,", text);
    }

    [Theory]
    [InlineData("+1+1")]
    [InlineData("+cmd")]
    public async Task FR_EXP_001_plus_text_that_is_not_a_phone_number_is_still_guarded(string value)
    {
        GivenPhone(value);
        GivenVisits(Visit(Morning));

        var text = TextOf((await service.BuildCsvAsync(7)).Value!.Content);

        Assert.Contains($",'{value},anna@example.test,", text);
    }

    [Fact]
    public async Task FR_EXP_001_numbers_are_not_changed_by_the_formula_guard()
    {
        GivenVisits(Visit(Morning, temperature: 36.8m, systolic: 120, diastolic: 80, pulse: 72));

        var text = TextOf((await service.BuildCsvAsync(7)).Value!.Content);

        Assert.Contains(",36.8,120,80,72,", text);
    }

    [Fact]
    public async Task FR_EXP_001_patient_with_no_visit_gives_one_row_with_blank_visit_columns()
    {
        var file = (await service.BuildCsvAsync(7)).Value!;

        var lines = TextOf(file.Content).Split("\r\n", StringSplitOptions.RemoveEmptyEntries);
        Assert.Equal(2, lines.Length);
        Assert.Equal("PT-000007,Test Anna Example,1990-05-01,Female,5550101,anna@example.test,Test address" + new string(',', 9),
            lines[1]);
    }

    [Fact]
    public async Task FR_EXP_001_csv_range_with_no_visits_is_refused_so_no_file_is_built()
    {
        var result = await service.BuildCsvAsync(7, new DateOnly(2026, 1, 1), new DateOnly(2026, 1, 31));

        Assert.False(result.IsSuccess);
        Assert.Equal(ExportService.NoVisitsInRangeCode, result.Errors[0].Code);
        Assert.Equal("No visits in this date range.", result.Errors[0].Message);
        Assert.Null(result.Value);
    }

    [Fact]
    public async Task FR_EXP_001_csv_a_to_date_without_a_from_date_is_refused_by_the_visit_range_rule()
    {
        var result = await service.BuildCsvAsync(7, null, new DateOnly(2026, 11, 10));

        Assert.Equal(VisitDateRange.FromMissingCode, result.Errors[0].Code);
        await consultations.DidNotReceive().ListForExportAsync(
            Arg.Any<int>(), Arg.Any<DateOnly?>(), Arg.Any<DateOnly?>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task FR_EXP_001_csv_a_from_date_after_the_to_date_is_refused_by_the_visit_range_rule()
    {
        var result = await service.BuildCsvAsync(7, new DateOnly(2026, 11, 12), new DateOnly(2026, 11, 10));

        Assert.Equal(VisitDateRange.OrderCode, result.Errors[0].Code);
        await consultations.DidNotReceive().ListForExportAsync(
            Arg.Any<int>(), Arg.Any<DateOnly?>(), Arg.Any<DateOnly?>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task FR_EXP_001_csv_file_name_carries_the_patient_code_only()
    {
        var file = (await service.BuildCsvAsync(7)).Value!;

        Assert.Equal("patient-PT-000007-visits.csv", file.FileName);
        Assert.DoesNotContain("Anna", file.FileName);
    }

    [Fact]
    public async Task FR_EXP_001_unknown_patient_is_not_found()
    {
        var result = await service.BuildCsvAsync(99);

        Assert.Equal(ExportService.PatientNotFoundCode, result.Errors[0].Code);
    }

    [Fact]
    public async Task FR_EXP_001_export_options_give_the_patient_for_the_form()
    {
        var options = (await service.GetOptionsAsync(7)).Value!;

        Assert.Equal("PT-000007", options.PatientCode);
        Assert.Equal("Test Anna Example", options.FullName);
    }

    [Fact]
    public async Task FR_EXP_002_print_view_lists_the_newest_visit_first()
    {
        GivenVisits(Visit(Morning.AddDays(-2), complaints: "Test older"), Visit(Morning, complaints: "Test newer"));

        var print = (await service.BuildPrintViewAsync(7)).Value!;

        Assert.Equal(7, print.PatientId);
        Assert.Equal(["Test newer", "Test older"], print.Visits.Select(v => v.Complaints));
    }

    [Fact]
    public async Task FR_EXP_002_print_view_with_a_single_day_shows_that_day_as_the_range()
    {
        GivenVisits(Visit(Morning));
        var day = new DateOnly(2026, 11, 3);

        var print = (await service.BuildPrintViewAsync(7, day)).Value!;

        Assert.Equal(day, print.From);
        Assert.Equal(day, print.To);
    }

    [Fact]
    public async Task FR_EXP_002_print_view_with_no_visits_in_a_range_is_refused()
    {
        var result = await service.BuildPrintViewAsync(7, new DateOnly(2026, 1, 1), new DateOnly(2026, 1, 31));

        Assert.Equal(ExportService.NoVisitsInRangeCode, result.Errors[0].Code);
    }
}
