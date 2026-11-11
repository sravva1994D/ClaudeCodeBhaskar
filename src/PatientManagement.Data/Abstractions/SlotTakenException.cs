namespace PatientManagement.Data.Abstractions;

/// <summary>
/// The database refused a second live appointment in the same slot (unique filtered index, PDR-07).
/// Raised when two bookings race past the service's overlap check.
/// </summary>
public sealed class SlotTakenException(Exception inner)
    : Exception("A live appointment already holds this slot.", inner);
