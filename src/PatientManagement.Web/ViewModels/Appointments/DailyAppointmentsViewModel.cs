using PatientManagement.Business.DTOs;

namespace PatientManagement.Web.ViewModels.Appointments;

// The daily list (SCR-10): one day of non-archived appointments in time order.
public sealed class DailyAppointmentsViewModel
{
    public DateOnly Date { get; set; }

    public IReadOnlyList<AppointmentDto> Items { get; set; } = [];
}
