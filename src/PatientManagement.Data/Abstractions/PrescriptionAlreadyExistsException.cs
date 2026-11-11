namespace PatientManagement.Data.Abstractions;

/// <summary>
/// The database refused a second prescription for the same consultation (unique index, FR-RX-002 AC-3).
/// Raised when two first saves of medicine lines race past the service's check.
/// </summary>
public sealed class PrescriptionAlreadyExistsException(Exception inner)
    : Exception("This consultation already has a prescription.", inner);
