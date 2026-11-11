using PatientManagement.Business.Common;
using PatientManagement.Business.DTOs;

namespace PatientManagement.Business.Interfaces;

// Prescription rules (Documentation/02-Planning/10-Service-Design.md §2.6; FR-RX-001, FR-RX-002; PDR-13, PDR-14).
public interface IPrescriptionService
{
    // Replaces the medicine lines of the visit's prescription, creating the prescription on the first save.
    // Blank lines are ignored. Refused once printed (AD-13).
    Task<Result<PrescriptionDto>> SaveItemsAsync(
        int consultationId, IReadOnlyList<MedicineLineInput> lines, CancellationToken cancellationToken = default);

    // The Print command: marks the prescription printed once and returns the print view. Refused without a medicine line (PDR-13).
    // Printing again returns the printed state and changes nothing (PDR-14).
    Task<Result<PrescriptionPrintDto>> MarkPrintedAsync(int consultationId, CancellationToken cancellationToken = default);

    // The print view for a visit. Read-only.
    Task<Result<PrescriptionPrintDto>> GetPrintModelAsync(int consultationId, CancellationToken cancellationToken = default);
}
