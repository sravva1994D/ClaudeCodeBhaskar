namespace PatientManagement.Data.Abstractions;

/// <summary>
/// The database refused a second visit for the same appointment (unique filtered index, PDR-26).
/// Raised when two starts race past the service's check.
/// </summary>
public sealed class AppointmentAlreadyVisitedException(Exception inner)
    : Exception("This appointment already has a consultation.", inner);
