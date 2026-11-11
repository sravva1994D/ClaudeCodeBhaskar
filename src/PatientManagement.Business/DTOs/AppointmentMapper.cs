using PatientManagement.Business.Appointments;
using PatientManagement.Data.Entities;

namespace PatientManagement.Business.DTOs;

// Maps appointment entities to DTOs. Entities never leave the Business layer (AD-03).
public static class AppointmentMapper
{
    public static AppointmentDto ToDto(Appointment appointment, Patient patient) => new(
        appointment.Id,
        appointment.PatientId,
        patient.PatientCode,
        patient.FullName,
        appointment.StartTime,
        appointment.DurationMinutes,
        appointment.Status,
        AppointmentLifecycle.ManualNextStatuses(appointment.Status),
        AppointmentLifecycle.CanArchive(appointment.Status),
        appointment.Status == AppointmentStatus.CheckedIn);
}
