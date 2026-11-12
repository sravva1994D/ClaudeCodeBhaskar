namespace PatientManagement.Business.DTOs;

// One row of the recent patients list on the home page (SCR-03; FR-SRC-002).
// LastVisit is the latest visit time, from visits only (PDR-11).
public sealed record RecentPatientDto(
    int Id,
    string PatientCode,
    string FullName,
    DateTime LastVisit);
