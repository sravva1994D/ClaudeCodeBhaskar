namespace PatientManagement.Data.Entities;

/// <summary>
/// A registered patient (FR-PAT-001 to FR-PAT-004). Age is not stored; it is calculated from DateOfBirth (BRD: Patient Management).
/// PatientCode is set by the database from the identity (AD-09, decision D-M2-1); the application never writes it.
/// </summary>
public class Patient
{
    public int Id { get; set; }

    public string PatientCode { get; set; } = string.Empty;

    public string FullName { get; set; } = string.Empty;

    public DateOnly DateOfBirth { get; set; }

    public string Gender { get; set; } = string.Empty;

    public string Phone { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string Address { get; set; } = string.Empty;
}
