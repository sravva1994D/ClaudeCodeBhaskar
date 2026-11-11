namespace PatientManagement.Business.DTOs;

// The clinic header as shown on SCR-14 and printed on the prescription (FR-RX-002, PDR-15).
public sealed record ClinicProfileDto(
    string? ClinicName,
    string? Address,
    string? Phone,
    string? DoctorName,
    string? RegistrationNumber)
{
    // The print view shows a notice while a header field is blank. Printing is still allowed (D-M5-6).
    public bool IsComplete =>
        new[] { ClinicName, Address, Phone, DoctorName, RegistrationNumber }.All(value => !string.IsNullOrWhiteSpace(value));
}

// What the user typed on SCR-14. Every field may be blank (D-M5-6).
public sealed record ClinicProfileInput(
    string? ClinicName,
    string? Address,
    string? Phone,
    string? DoctorName,
    string? RegistrationNumber);
