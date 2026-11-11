using PatientManagement.Data.Entities;

namespace PatientManagement.Data.Abstractions;

// Thin access to vital signs (Documentation/02-Planning/11-Repository-Design.md §2.5; FR-VIT-001).
public interface IVitalSignRepository
{
    // Saves the vitals for a visit: adds the row the first time, and updates it after that (1:0..1).
    Task UpsertAsync(VitalSign vitals, CancellationToken cancellationToken = default);
}
