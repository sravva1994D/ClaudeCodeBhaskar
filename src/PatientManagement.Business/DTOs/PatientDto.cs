namespace PatientManagement.Business.DTOs;

// The patient profile (SCR-06): every captured field, plus the calculated age (FR-PAT-003).
public sealed record PatientDto(
    int Id,
    string PatientCode,
    string FullName,
    DateOnly DateOfBirth,
    int Age,
    string Gender,
    string Phone,
    string Email,
    string Address);
