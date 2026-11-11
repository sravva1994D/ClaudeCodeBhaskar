namespace PatientManagement.Business.DTOs;

// One medicine line as typed on the consultation screen (FR-RX-001). Blank fields are null.
public sealed record MedicineLineInput(string? Name, string? Dosage, string? Frequency, string? Duration, string? Instructions);

// One medicine line as shown on the prescription (FR-RX-001). Lines are in display order.
public sealed record MedicineLineDto(string Name, string? Dosage, string? Frequency, string? Duration, string? Instructions);

// The prescription of a visit (FR-RX-001, FR-RX-002).
public sealed record PrescriptionDto(
    int Id,
    int ConsultationId,
    bool IsPrinted,
    DateTime? PrintedAt,
    IReadOnlyList<MedicineLineDto> Lines);

// Everything the printable A4 prescription shows (FR-RX-002 AC-2). The clinic header comes from SCR-14 (PDR-15).
public sealed record PrescriptionPrintDto(
    int ConsultationId,
    int PrescriptionId,
    bool IsPrinted,
    DateTime? PrintedAt,
    ClinicProfileDto Clinic,
    string PatientName,
    string PatientCode,
    DateOnly DateOfBirth,
    string Gender,
    DateTime VisitDateTime,
    VitalsDto? Vitals,
    string? Diagnosis,
    IReadOnlyList<MedicineLineDto> Lines);
