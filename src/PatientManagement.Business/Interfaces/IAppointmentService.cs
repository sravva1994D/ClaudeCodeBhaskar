using PatientManagement.Business.Common;
using PatientManagement.Business.DTOs;

namespace PatientManagement.Business.Interfaces;

// Appointment rules (Documentation/02-Planning/10-Service-Design.md §2.3; FR-APT-001 to FR-APT-003).
// Errors come back in the Result; nothing is saved when there are errors.
public interface IAppointmentService
{
    Task<Result<AppointmentDto>> ScheduleAsync(AppointmentCreateDto input, CancellationToken cancellationToken = default);

    // Non-archived appointments on the given local date, in time order.
    Task<IReadOnlyList<AppointmentDto>> GetDailyAsync(DateOnly date, CancellationToken cancellationToken = default);

    // Every grid slot for the date, marked taken when a live appointment holds it.
    Task<IReadOnlyList<SlotDto>> GetSlotsAsync(DateOnly date, CancellationToken cancellationToken = default);

    Task<Result<AppointmentDto>> UpdateStatusAsync(int id, string status, CancellationToken cancellationToken = default);

    // Archives a finished or released appointment. It does not change the status (PDR-09).
    Task<Result<AppointmentDto>> ArchiveAsync(int id, CancellationToken cancellationToken = default);

    // The appointment to reschedule, for the form. Only a Scheduled appointment can be rescheduled (FR-APT-004).
    Task<Result<AppointmentDto>> GetForRescheduleAsync(int id, CancellationToken cancellationToken = default);

    // Moves a Scheduled appointment to another free slot for the same patient (FR-APT-004, DEC-027).
    Task<Result<AppointmentDto>> RescheduleAsync(int id, DateTime newStartTime, DateOnly? shownDay = null, CancellationToken cancellationToken = default);
}
