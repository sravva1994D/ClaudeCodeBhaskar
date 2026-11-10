namespace PatientManagement.Data.Entities;

/// <summary>
/// The single physician account (NFR-SEC-001, CON-002). Created once by the console seed command (DEC-018).
/// The password is never stored; only its hash is (NFR-SEC-002).
/// </summary>
public class User
{
    public int Id { get; set; }

    public string Username { get; set; } = string.Empty;

    public string PasswordHash { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;
}
