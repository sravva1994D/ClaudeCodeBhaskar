namespace PatientManagement.Data.Entities;

/// <summary>
/// Header data for printed prescriptions (FR-RX-002, PDR-15). A single row with Id = 1 (database CHECK).
/// Nullable because the BRD does not say when the header must be complete. The print view shows a notice when it is incomplete (D-M5-6).
/// </summary>
public class ClinicProfile
{
    public int Id { get; set; }

    public string? ClinicName { get; set; }

    public string? Address { get; set; }

    public string? Phone { get; set; }

    public string? DoctorName { get; set; }

    public string? RegistrationNumber { get; set; }
}
