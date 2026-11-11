using PatientManagement.Business.DTOs;

namespace PatientManagement.Web.ViewModels.Home;

// The home page: today's non-archived appointments (SCR-03).
public sealed class HomeViewModel
{
    public DateOnly Today { get; set; }

    public IReadOnlyList<AppointmentDto> Appointments { get; set; } = [];
}
