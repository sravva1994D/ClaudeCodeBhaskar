using PatientManagement.Data.Entities;

namespace PatientManagement.Data.Abstractions;

// Thin access to appointments (Documentation/02-Planning/11-Repository-Design.md §2.3).
// Rules (slot grid, transitions, archive eligibility) live in the Business layer. Repositories save what they are given.
public interface IAppointmentRepository
{
    Task<Appointment?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    // Non-archived appointments starting on the given local date, ordered by start time.
    // Each row includes its patient, for the name and code shown on the list (FR-APT-002).
    Task<IReadOnlyList<Appointment>> GetDailyAsync(DateOnly date, CancellationToken cancellationToken = default);

    // True when a non-archived appointment that holds its slot (PDR-07) already starts at startTime.
    // The appointment with excludeId is ignored.
    Task<bool> HasOverlapAsync(DateTime startTime, int? excludeId = null, CancellationToken cancellationToken = default);

    // Adds the appointment and saves it.
    Task AddAsync(Appointment appointment, CancellationToken cancellationToken = default);

    // Saves the entity's current values (for example a status change or archiving).
    Task UpdateAsync(Appointment appointment, CancellationToken cancellationToken = default);
}
