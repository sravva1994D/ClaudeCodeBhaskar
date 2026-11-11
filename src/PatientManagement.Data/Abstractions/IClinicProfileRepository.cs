using PatientManagement.Data.Entities;

namespace PatientManagement.Data.Abstractions;

// Thin access to the single clinic profile row (FR-RX-002, PDR-15).
public interface IClinicProfileRepository
{
    // The single row, or null when it has not been created yet.
    Task<ClinicProfile?> GetAsync(CancellationToken cancellationToken = default);

    // Saves the header. Always writes the row with Id = 1, adding it the first time.
    Task UpsertAsync(ClinicProfile profile, CancellationToken cancellationToken = default);
}
