using PatientManagement.Business.Validators;

namespace PatientManagement.Business.DTOs;

// A visit as shown on the consultation screen (SCR-11) and the visit detail (SCR-08). FR-HIS-002, FR-VIT-001, FR-CMP-001, FR-DIA-001.
public sealed record ConsultationDto(
    int Id,
    int PatientId,
    string PatientName,
    string PatientCode,
    int? AppointmentId,
    DateTime VisitDateTime,
    string? Complaints,
    string? Diagnosis,
    DateTime? FinishedAt,
    VitalsDto? Vitals)
{
    public bool IsFinished => FinishedAt is not null;
}

// The vitals of a visit, when they were saved (FR-VIT-001).
public sealed record VitalsDto(decimal TemperatureC, int SystolicMmHg, int DiastolicMmHg, int PulseBpm);

// What the physician entered on the consultation screen. Vitals may be all empty (FR-VIT-001 AC-2).
public sealed record ConsultationSaveInput(string? Complaints, string? Diagnosis, VitalsInput Vitals);

// One line of the visit history (SCR-07). The date filter comes in Module 6 (FR-HIS-003).
public sealed record VisitSummaryDto(int Id, DateTime VisitDateTime, int? AppointmentId, bool IsFinished);
