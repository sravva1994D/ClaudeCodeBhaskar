namespace PatientManagement.Business.DTOs;

// One row of the search results (SCR-04): name, code, date of birth and phone, plus the Id for the link to the profile.
public sealed record PatientSummaryDto(
    int Id,
    string PatientCode,
    string FullName,
    DateOnly DateOfBirth,
    string Phone);
