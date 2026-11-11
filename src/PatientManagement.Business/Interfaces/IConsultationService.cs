using PatientManagement.Business.Common;
using PatientManagement.Business.DTOs;

namespace PatientManagement.Business.Interfaces;

// Visit rules (Documentation/02-Planning/10-Service-Design.md §2.4, §2.5; FR-VIT-001, FR-CMP-001, FR-DIA-001, FR-HIS-001, FR-HIS-002, FR-APT-003).
// Errors come back in the Result. Range warnings come back as a confirmation request, and nothing is saved until the user confirms (AD-11).
public interface IConsultationService
{
    // Starts a visit for a patient. With an appointmentId, the appointment must be checked in and not yet linked; it becomes In Consultation (PDR-26, D-M4-1).
    Task<Result<ConsultationDto>> StartAsync(int patientId, int? appointmentId, CancellationToken cancellationToken = default);

    // The visit with its vitals, for the consultation screen and the visit detail.
    Task<Result<ConsultationDto>> GetAsync(int consultationId, CancellationToken cancellationToken = default);

    // Saves complaints, diagnosis and vitals. Out-of-range vitals need confirmation. Empty vitals leave saved vitals unchanged.
    Task<Result<ConsultationDto>> SaveAsync(
        int consultationId, ConsultationSaveInput input, bool confirmWarnings, CancellationToken cancellationToken = default);

    // Finishes a visit once (D-M4-3). For an appointment visit, the appointment must be In Consultation and becomes Completed.
    Task<Result<ConsultationDto>> FinishAsync(int consultationId, CancellationToken cancellationToken = default);

    // All visits for a patient, newest first (FR-HIS-001).
    Task<IReadOnlyList<VisitSummaryDto>> ListVisitsAsync(int patientId, CancellationToken cancellationToken = default);
}
