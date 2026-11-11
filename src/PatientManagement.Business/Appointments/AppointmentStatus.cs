namespace PatientManagement.Business.Appointments;

// The six lifecycle values (Documentation/02-Planning/19-Appointment-Status-Lifecycle.md §2; FR-APT-003).
// The database CHECK constraint lists the same values (AppointmentConfiguration).
public static class AppointmentStatus
{
    public const string Scheduled = "Scheduled";
    public const string CheckedIn = "Checked-In";
    public const string InConsultation = "In Consultation";
    public const string Completed = "Completed";
    public const string Cancelled = "Cancelled";
    public const string NoShow = "No-show";

    public static readonly IReadOnlyList<string> All =
        [Scheduled, CheckedIn, InConsultation, Completed, Cancelled, NoShow];

    public static bool IsKnown(string? status) => status is not null && All.Contains(status);

    // PDR-07: these statuses hold the slot. Cancelled and No-show release it.
    public static bool HoldsSlot(string status) =>
        status is Scheduled or CheckedIn or InConsultation or Completed;
}
