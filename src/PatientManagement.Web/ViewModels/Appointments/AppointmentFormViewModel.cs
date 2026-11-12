namespace PatientManagement.Web.ViewModels.Appointments;

// The schedule screen (SCR-09). Form fields are nullable so the service's messages show, not MVC's required messages
// (13-Validation-Strategy §5). Name and code are display values; only PatientId and the chosen slot are posted.
public sealed class AppointmentFormViewModel
{
    public int PatientId { get; set; }

    public string? PatientName { get; set; }

    public string? PatientCode { get; set; }

    public DateOnly? Date { get; set; }

    public DateTime? StartTime { get; set; }

    public IReadOnlyList<SlotViewModel> Slots { get; set; } = [];

    // Reschedule only (FR-APT-004): the appointment being moved and its current time, shown for reference.
    public int? AppointmentId { get; set; }

    public DateTime? CurrentStartTime { get; set; }
}

// One grid slot. A booked slot is shown but cannot be chosen (PDR-07).
public sealed record SlotViewModel(DateTime StartTime, string Label, bool IsTaken);
