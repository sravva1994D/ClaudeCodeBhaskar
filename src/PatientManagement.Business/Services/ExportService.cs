using System.Globalization;
using PatientManagement.Business.Common;
using PatientManagement.Business.DTOs;
using PatientManagement.Business.Interfaces;
using PatientManagement.Data.Abstractions;
using PatientManagement.Data.Entities;

namespace PatientManagement.Business.Services;

// Export content and rules for one patient (FR-EXP-001, FR-EXP-002; PDR-17, PDR-18, DEC-020; D-M7-1 to D-M7-8).
public sealed class ExportService(
    IPatientRepository patients,
    IConsultationRepository consultations,
    IClinicProfileRepository clinicProfiles,
    ICsvWriter csvWriter) : IExportService
{
    public const string PatientNotFoundCode = "Export.Patient.NotFound";
    public const string PatientNotFoundMessage = "The patient was not found.";
    public const string NoVisitsInRangeCode = "Export.NoVisitsInRange";
    public const string NoVisitsInRangeMessage = "No visits in this date range.";

    // The 16 columns of the CSV, in this order (10-Service-Design.md §2.7). Patient columns first, then visit columns.
    private static readonly string[] CsvHeader =
    [
        "PatientCode", "FullName", "DateOfBirth", "Gender", "Phone", "Email", "Address",
        "VisitDate", "VisitTime", "TemperatureC", "SystolicMmHg", "DiastolicMmHg", "PulseBpm",
        "Complaints", "Diagnosis", "Medicines"
    ];

    private const int VisitColumnCount = 9;

    public async Task<Result<ExportOptionsDto>> GetOptionsAsync(int patientId, CancellationToken cancellationToken = default)
    {
        var patient = await patients.GetByIdAsync(patientId, cancellationToken);
        if (patient is null)
        {
            return Result<ExportOptionsDto>.Fail(PatientNotFound());
        }

        return Result<ExportOptionsDto>.Ok(new ExportOptionsDto(patient.Id, patient.PatientCode, patient.FullName));
    }

    public async Task<Result<ExportFile>> BuildCsvAsync(
        int patientId, DateOnly? from = null, DateOnly? to = null, CancellationToken cancellationToken = default)
    {
        var loaded = await LoadAsync(patientId, from, to, cancellationToken);
        if (!loaded.IsSuccess)
        {
            return Result<ExportFile>.Fail(loaded.Errors.ToArray());
        }

        var (patient, visits) = loaded.Value!;
        IReadOnlyList<IReadOnlyList<string>> rows = visits.Count == 0
            // A patient with no visits gives one row: the patient columns, and blank visit columns (10 §2.7).
            ? [[.. PatientColumns(patient), .. Enumerable.Repeat("", VisitColumnCount)]]
            : visits.Select(visit => (IReadOnlyList<string>)[.. PatientColumns(patient), .. VisitColumns(visit)]).ToList();

        var bytes = csvWriter.Write(CsvHeader, rows);

        return Result<ExportFile>.Ok(new ExportFile(bytes, $"patient-{patient.PatientCode}-visits.csv"));
    }

    public async Task<Result<ExportPrintDto>> BuildPrintViewAsync(
        int patientId, DateOnly? from = null, DateOnly? to = null, CancellationToken cancellationToken = default)
    {
        var loaded = await LoadAsync(patientId, from, to, cancellationToken);
        if (!loaded.IsSuccess)
        {
            return Result<ExportPrintDto>.Fail(loaded.Errors.ToArray());
        }

        var (patient, visits) = loaded.Value!;
        var clinic = ClinicProfileMapper.ToDto(await clinicProfiles.GetAsync(cancellationToken));

        // The printed view lists the newest visit first, like the visit history (D-M7-7).
        return Result<ExportPrintDto>.Ok(new ExportPrintDto(
            clinic,
            patient.FullName,
            patient.PatientCode,
            patient.DateOfBirth,
            patient.Gender,
            patient.Phone,
            patient.Email,
            patient.Address,
            from,
            VisitDateRange.Upper(from, to),
            visits.Reverse().ToList()));
    }

    // The patient and the visits in the range, oldest first. A range with no visits is refused (D-M7-6).
    private async Task<Result<(Patient Patient, IReadOnlyList<ExportVisitDto> Visits)>> LoadAsync(
        int patientId, DateOnly? from, DateOnly? to, CancellationToken cancellationToken)
    {
        var patient = await patients.GetByIdAsync(patientId, cancellationToken);
        if (patient is null)
        {
            return Result<(Patient, IReadOnlyList<ExportVisitDto>)>.Fail(PatientNotFound());
        }

        if (VisitDateRange.Validate(from, to) is { } refusal)
        {
            return Result<(Patient, IReadOnlyList<ExportVisitDto>)>.Fail(refusal);
        }

        var rows = await consultations.ListForExportAsync(patientId, from, VisitDateRange.Upper(from, to), cancellationToken);
        if (rows.Count == 0 && from is not null)
        {
            return Result<(Patient, IReadOnlyList<ExportVisitDto>)>.Fail(
                new ResultMessage(NoVisitsInRangeCode, NoVisitsInRangeMessage));
        }

        IReadOnlyList<ExportVisitDto> visits = rows.Select(ToVisit).ToList();

        return Result<(Patient, IReadOnlyList<ExportVisitDto>)>.Ok((patient, visits));
    }

    private static ExportVisitDto ToVisit(ExportVisitRow row) => new(
        row.VisitDateTime,
        row.TemperatureC,
        row.SystolicMmHg,
        row.DiastolicMmHg,
        row.PulseBpm,
        row.Complaints,
        row.Diagnosis,
        row.Medicines.Select(medicine => new MedicineLineDto(
            medicine.Name, medicine.Dosage, medicine.Frequency, medicine.Duration, medicine.Instructions)).ToList());

    // Patient columns 1 to 7. Dates use yyyy-MM-dd (DEC-020).
    private static string[] PatientColumns(Patient patient) =>
    [
        Text(patient.PatientCode),
        Text(patient.FullName),
        patient.DateOfBirth.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
        Text(patient.Gender),
        Text(patient.Phone),
        Text(patient.Email),
        Text(patient.Address)
    ];

    // Visit columns 8 to 16. Vitals are blank when not recorded. Temperature has one decimal place.
    private static string[] VisitColumns(ExportVisitDto visit) =>
    [
        visit.VisitDateTime.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
        visit.VisitDateTime.ToString("HH:mm", CultureInfo.InvariantCulture),
        visit.TemperatureC?.ToString("0.0", CultureInfo.InvariantCulture) ?? "",
        visit.SystolicMmHg?.ToString(CultureInfo.InvariantCulture) ?? "",
        visit.DiastolicMmHg?.ToString(CultureInfo.InvariantCulture) ?? "",
        visit.PulseBpm?.ToString(CultureInfo.InvariantCulture) ?? "",
        Text(visit.Complaints),
        Text(visit.Diagnosis),
        Text(MedicinesCell(visit.Medicines))
    ];

    // Each medicine as "Name; Dosage; Frequency; Duration; Instructions", blank parts omitted; medicines joined by " | " (10 §2.7).
    private static string MedicinesCell(IReadOnlyList<MedicineLineDto> medicines) =>
        string.Join(" | ", medicines.Select(medicine => string.Join("; ",
            new[] { medicine.Name, medicine.Dosage, medicine.Frequency, medicine.Duration, medicine.Instructions }
                .Where(part => !string.IsNullOrWhiteSpace(part)))));

    // Free text in a spreadsheet: a cell that starts with a formula character gets a leading quote, so the spreadsheet
    // shows it as text and does not run it (D-M7-3). Blank text is an empty cell.
    private static string Text(string? value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return "";
        }

        return value[0] is '=' or '+' or '-' or '@' or '\t' or '\r' ? "'" + value : value;
    }

    private static ResultMessage PatientNotFound() => new(PatientNotFoundCode, PatientNotFoundMessage);
}
