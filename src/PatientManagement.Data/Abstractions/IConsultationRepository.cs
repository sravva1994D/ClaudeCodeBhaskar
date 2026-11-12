using PatientManagement.Data.Entities;

namespace PatientManagement.Data.Abstractions;

// One patient with the time of their latest visit (FR-SRC-002).
public sealed record PatientLastVisit(int PatientId, DateTime LastVisit);

// One medicine line of a visit's prescription, in display order (FR-EXP-001, FR-RX-001).
public sealed record ExportMedicine(string Name, string? Dosage, string? Frequency, string? Duration, string? Instructions);

// One visit with its vitals and medicines, for export (FR-EXP-001, FR-EXP-002). Vitals fields are null when not recorded.
public sealed record ExportVisitRow(
    int VisitId,
    DateTime VisitDateTime,
    string? Complaints,
    string? Diagnosis,
    decimal? TemperatureC,
    short? SystolicMmHg,
    short? DiastolicMmHg,
    short? PulseBpm,
    IReadOnlyList<ExportMedicine> Medicines);

// Thin access to visits (Documentation/02-Planning/11-Repository-Design.md §2.4).
// Rules (start, finish, editing) live in the Business layer. Repositories save what they are given.
public interface IConsultationRepository
{
    Task<Consultation?> GetByIdAsync(int id, bool includeVitals = false, CancellationToken cancellationToken = default);

    // The visit linked to an appointment, if one exists (PDR-26).
    Task<Consultation?> GetByAppointmentIdAsync(int appointmentId, CancellationToken cancellationToken = default);

    // All visits for one patient, newest first (FR-HIS-001). from and to are local calendar days, both inclusive
    // (FR-HIS-003): a visit on the to day is kept whatever its time. A single day is from = to. Null means no bound.
    Task<IReadOnlyList<Consultation>> ListByPatientAsync(int patientId, DateOnly? from = null, DateOnly? to = null,
        CancellationToken cancellationToken = default);

    // The time of the latest visit, from visits only (PDR-11). Null when the patient has no visit.
    Task<DateTime?> GetLatestVisitTimeAsync(int patientId, CancellationToken cancellationToken = default);

    // The most recently visited patients, latest visit first, at most count (FR-SRC-002, PDR-11).
    // Patients with no visit are not returned.
    Task<IReadOnlyList<PatientLastVisit>> GetRecentPatientVisitsAsync(int count, CancellationToken cancellationToken = default);

    // The visits of one patient for export, oldest first (D-M7-7), with vitals and medicines. The same day rules as
    // ListByPatientAsync apply (FR-EXP-001 uses the filter of FR-HIS-003). Returns nothing when the patient has no visit
    // in the range. The service adds the patient's own details.
    Task<IReadOnlyList<ExportVisitRow>> ListForExportAsync(int patientId, DateOnly? from = null, DateOnly? to = null,
        CancellationToken cancellationToken = default);

    // Adds the visit. When linkedAppointment is given, its status change is saved in the same save,
    // so Start either changes both or neither (Module 4 plan §7).
    Task AddAsync(Consultation consultation, Appointment? linkedAppointment = null, CancellationToken cancellationToken = default);

    // Saves the visit's text and its vitals (1:0..1) in one save (D-M5-8). Returns false, and saves nothing, when the
    // visit's prescription is already printed (FR-RX-002 AC-4). The visit must be tracked by this context.
    Task<bool> UpdateAsync(Consultation consultation, CancellationToken cancellationToken = default);

    // Finishes the visit once (D-M4-3). Returns false when it was already finished, so two overlapping
    // Finish requests cannot both succeed. When linkedAppointment is given, its status is saved in the same transaction.
    Task<bool> MarkFinishedAsync(int consultationId, DateTime finishedAt, Appointment? linkedAppointment = null,
        CancellationToken cancellationToken = default);
}
