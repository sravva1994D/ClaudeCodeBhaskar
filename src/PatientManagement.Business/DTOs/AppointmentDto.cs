namespace PatientManagement.Business.DTOs;

// A booked appointment as shown on the daily list and after a change (SCR-09, SCR-10; FR-APT-001, FR-APT-002).
public sealed record AppointmentDto(
    int Id,
    int PatientId,
    string PatientCode,
    string PatientName,
    DateTime StartTime,
    int DurationMinutes,
    string Status);

// The input to book an appointment (SCR-09).
public sealed record AppointmentCreateDto(int PatientId, DateTime StartTime);

// One grid slot for a day, and whether a live appointment holds it (SCR-09 slot picker; PDR-07).
public sealed record SlotDto(DateTime StartTime, bool IsTaken);
