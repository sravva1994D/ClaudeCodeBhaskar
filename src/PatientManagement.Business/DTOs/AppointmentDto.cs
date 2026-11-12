namespace PatientManagement.Business.DTOs;

// A booked appointment as shown on the daily list and after a change (SCR-09, SCR-10; FR-APT-001, FR-APT-002).
// NextStatuses and CanArchive tell the list which actions to offer; the rules themselves stay in the Business layer.
public sealed record AppointmentDto(
    int Id,
    int PatientId,
    string PatientCode,
    string PatientName,
    DateTime StartTime,
    int DurationMinutes,
    string Status,
    IReadOnlyList<string> NextStatuses,
    bool CanArchive,
    bool CanStartConsultation);

// The input to book an appointment (SCR-09).
// ShownDay is the day in the Day box when the slot was chosen. Null skips the same-day check (for example, in code that has no form).
public sealed record AppointmentCreateDto(int PatientId, DateTime StartTime, DateOnly? ShownDay = null);

// One grid slot for a day, and whether a live appointment holds it (SCR-09 slot picker; PDR-07).
public sealed record SlotDto(DateTime StartTime, bool IsTaken);
