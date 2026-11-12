using PatientManagement.Data.Entities;

namespace PatientManagement.Data.Abstractions;

// Thin access to patients (Documentation/02-Planning/11-Repository-Design.md §2.2).
// Rules live in the Business layer. Repositories save what they are given.
public interface IPatientRepository
{
    Task<Patient?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    // The patients with the given ids, in no particular order. Ids that match no patient are skipped (FR-SRC-002).
    Task<IReadOnlyList<Patient>> GetByIdsAsync(IReadOnlyCollection<int> ids, CancellationToken cancellationToken = default);

    // Contains match on name or phone, case-insensitive. The term is taken literally (PDR-23).
    Task<IReadOnlyList<Patient>> SearchAsync(string term, CancellationToken cancellationToken = default);

    // Exact match on name (case-insensitive), date of birth, or phone (PDR-10). The patient with excludeId is ignored.
    Task<IReadOnlyList<Patient>> FindPossibleDuplicatesAsync(
        string fullName, DateOnly dateOfBirth, string phone, int? excludeId = null,
        CancellationToken cancellationToken = default);

    // Adds the patient and saves it. The identity and the patient code are set after the save.
    Task AddAsync(Patient patient, CancellationToken cancellationToken = default);

    // Saves the entity's current values. The patient code is not changed.
    Task UpdateAsync(Patient patient, CancellationToken cancellationToken = default);
}
