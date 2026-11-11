using PatientManagement.Data.Entities;

namespace PatientManagement.Data.Abstractions;

// Thin access to prescriptions and medicine lines (Documentation/02-Planning/11-Repository-Design.md; FR-RX-001, FR-RX-002).
// Rules (blank lines, the medicine minimum, the printed lock) live in the Business layer. The conditional writes below
// are the database backstop for races: each one changes nothing when its condition no longer holds.
public interface IPrescriptionRepository
{
    // The prescription for a consultation with its lines in display order, or null when none exists.
    Task<Prescription?> GetByConsultationAsync(int consultationId, CancellationToken cancellationToken = default);

    // Adds the prescription. Throws PrescriptionAlreadyExistsException when the consultation already has one.
    Task AddAsync(Prescription prescription, CancellationToken cancellationToken = default);

    // Replaces all lines of an unprinted prescription in one transaction. The lines passed in are new rows.
    // Returns false, and changes nothing, when the prescription is missing or already printed (AD-13).
    Task<bool> ReplaceItemsAsync(int prescriptionId, IReadOnlyList<PrescriptionItem> items,
        CancellationToken cancellationToken = default);

    // Marks the prescription printed, once (PDR-14). Returns false, and changes nothing, when it is missing,
    // already printed, or has no lines (PDR-13). A second print keeps the first PrintedAt.
    Task<bool> MarkPrintedAsync(int prescriptionId, DateTime printedAt, CancellationToken cancellationToken = default);
}
