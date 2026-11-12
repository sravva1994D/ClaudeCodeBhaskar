using PatientManagement.Business.DTOs;

namespace PatientManagement.Web.ViewModels.Home;

// The home page (SCR-03): today's non-archived appointments and the five most recently visited patients (FR-SRC-002).
public sealed class HomeViewModel
{
    public DateOnly Today { get; set; }

    public IReadOnlyList<AppointmentDto> Appointments { get; set; } = [];

    public IReadOnlyList<RecentPatientDto> RecentPatients { get; set; } = [];
}
