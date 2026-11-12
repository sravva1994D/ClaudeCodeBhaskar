using PatientManagement.Business.Common;

namespace PatientManagement.Business.DTOs;

// The export form for one patient (SCR-13, FR-EXP-001, FR-EXP-002).
public sealed record ExportOptionsDto(int PatientId, string PatientCode, string FullName);

// A file for download: its bytes and its name (FR-EXP-001 AC-1). The name carries the patient code only (D-M7-4).
public sealed record ExportFile(byte[] Content, string FileName);

// One visit in an export, with its vitals and medicines (FR-EXP-001, FR-EXP-002). Vitals are null when not recorded.
public sealed record ExportVisitDto(
    DateTime VisitDateTime,
    decimal? TemperatureC,
    short? SystolicMmHg,
    short? DiastolicMmHg,
    short? PulseBpm,
    string? Complaints,
    string? Diagnosis,
    IReadOnlyList<MedicineLineDto> Medicines);

// Everything the printable export view shows (FR-EXP-002). Visits are newest first (D-M7-7).
public sealed record ExportPrintDto(
    ClinicProfileDto Clinic,
    string PatientName,
    string PatientCode,
    DateOnly DateOfBirth,
    string Gender,
    string Phone,
    string Email,
    string Address,
    DateOnly? From,
    DateOnly? To,
    IReadOnlyList<ExportVisitDto> Visits);
