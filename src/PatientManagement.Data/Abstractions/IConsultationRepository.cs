using PatientManagement.Data.Entities;

namespace PatientManagement.Data.Abstractions;

// Thin access to visits (Documentation/02-Planning/11-Repository-Design.md §2.4).
// Rules (start, finish, editing) live in the Business layer. Repositories save what they are given.
public interface IConsultationRepository
{
    Task<Consultation?> GetByIdAsync(int id, bool includeVitals = false, CancellationToken cancellationToken = default);

    // The visit linked to an appointment, if one exists (PDR-26).
    Task<Consultation?> GetByAppointmentIdAsync(int appointmentId, CancellationToken cancellationToken = default);

    // All visits for one patient, newest first (FR-HIS-001). The date filter comes in Module 6 (FR-HIS-003).
    Task<IReadOnlyList<Consultation>> ListByPatientAsync(int patientId, CancellationToken cancellationToken = default);

    // The time of the latest visit, from visits only (PDR-11). Null when the patient has no visit.
    Task<DateTime?> GetLatestVisitTimeAsync(int patientId, CancellationToken cancellationToken = default);

    // Adds the visit. When linkedAppointment is given, its status change is saved in the same save,
    // so Start either changes both or neither (Module 4 plan §7).
    Task AddAsync(Consultation consultation, Appointment? linkedAppointment = null, CancellationToken cancellationToken = default);

    // Saves the visit's text. Vitals are saved by IVitalSignRepository.
    Task UpdateAsync(Consultation consultation, CancellationToken cancellationToken = default);

    // Finishes the visit once (D-M4-3). Returns false when it was already finished, so two overlapping
    // Finish requests cannot both succeed. When linkedAppointment is given, its status is saved in the same transaction.
    Task<bool> MarkFinishedAsync(int consultationId, DateTime finishedAt, Appointment? linkedAppointment = null,
        CancellationToken cancellationToken = default);
}
