namespace PatientManagement.Business.DTOs;

// What the physician enters for a patient, for both registration and editing (Documentation/02-Planning/05-Screen-Catalog.md SCR-05).
// Fields are nullable so that a missing value can be reported as an error, not a crash. The patient code is never input.
public sealed record PatientInput(
    string? FullName,
    DateOnly? DateOfBirth,
    string? Gender,
    string? Phone,
    string? Email,
    string? Address);
