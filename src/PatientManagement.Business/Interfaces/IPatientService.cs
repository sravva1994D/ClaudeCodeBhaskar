using PatientManagement.Business.Common;
using PatientManagement.Business.DTOs;

namespace PatientManagement.Business.Interfaces;

// Patient rules (Documentation/02-Planning/10-Service-Design.md §2.2; BRD: Patient Management).
// Possible duplicates return a confirmation request; nothing is saved until the user confirms.
public interface IPatientService
{
    Task<Result<PatientDto>> RegisterAsync(PatientInput input, bool confirmWarnings, CancellationToken cancellationToken = default);

    Task<Result<PatientDto>> UpdateAsync(int id, PatientInput input, bool confirmWarnings, CancellationToken cancellationToken = default);

    Task<Result<PatientDto>> GetAsync(int id, CancellationToken cancellationToken = default);

    Task<Result<IReadOnlyList<PatientSummaryDto>>> SearchAsync(string? term, CancellationToken cancellationToken = default);

    // The most recently visited patients, latest visit first, at most count (FR-SRC-002, PDR-11). Patients with no visit are not listed.
    Task<IReadOnlyList<RecentPatientDto>> GetRecentAsync(int count = 5, CancellationToken cancellationToken = default);
}
