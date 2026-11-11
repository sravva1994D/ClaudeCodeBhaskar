namespace PatientManagement.Business.Appointments;

// The transition table (Documentation/02-Planning/19-Appointment-Status-Lifecycle.md §2; PDR-08, PDR-27, DEC-022).
// Kept in the Business layer, not in the controller or the database (19 §6).
public static class AppointmentLifecycle
{
    // Every move the lifecycle allows. Terminal statuses (Completed, Cancelled, No-show) have no moves out.
    private static readonly (string From, string To)[] Moves =
    [
        (AppointmentStatus.Scheduled, AppointmentStatus.CheckedIn),
        (AppointmentStatus.Scheduled, AppointmentStatus.Cancelled),
        (AppointmentStatus.Scheduled, AppointmentStatus.NoShow),
        (AppointmentStatus.CheckedIn, AppointmentStatus.InConsultation),
        (AppointmentStatus.CheckedIn, AppointmentStatus.Cancelled),
        (AppointmentStatus.InConsultation, AppointmentStatus.Completed)
    ];

    // Set only by the consultation Start and Finish actions (M4; PDR-27, DEC-022), never by a manual status change.
    private static readonly string[] SystemOnlyTargets =
        [AppointmentStatus.InConsultation, AppointmentStatus.Completed];

    // Archive is limited to appointments that are finished or released. Archiving a live appointment would free its slot silently.
    private static readonly string[] Archivable =
        [AppointmentStatus.Completed, AppointmentStatus.Cancelled, AppointmentStatus.NoShow];

    public static bool CanMove(string from, string to) => Moves.Contains((from, to));

    // The move a user can make by hand in Phase 1 M3 (decision D-M3-2).
    public static bool CanMoveManually(string from, string to) =>
        CanMove(from, to) && !SystemOnlyTargets.Contains(to);

    // The statuses a user can move to by hand from this one. The list screen offers exactly these.
    public static IReadOnlyList<string> ManualNextStatuses(string from) =>
        AppointmentStatus.All.Where(to => CanMoveManually(from, to)).ToList();

    public static bool CanArchive(string status) => Archivable.Contains(status);
}
