using PatientManagement.Business.Appointments;
using PatientManagement.Business.Common;

namespace PatientManagement.Business.Validators;

// Appointment rules (Documentation/02-Planning/13-Validation-Strategy.md §3.3; PDR-06, PDR-07, PDR-08, PDR-09).
// Returns errors only. Overlap needs the database and is checked by AppointmentService.
public static class AppointmentValidator
{
    // Decision D-M3-1: a booking date cannot be before today. Earlier slots today are allowed.
    public static IReadOnlyList<ResultMessage> ValidateSchedule(DateTime startTime, DateOnly today)
    {
        var errors = new List<ResultMessage>();

        if (!SlotGrid.IsOnGrid(startTime))
        {
            errors.Add(new ResultMessage("Appointment.StartTime.NotOnGrid",
                "Choose a 30-minute slot between 09:00 and 12:30, or between 17:00 and 19:30.", "StartTime"));
        }

        if (DateOnly.FromDateTime(startTime) < today)
        {
            errors.Add(new ResultMessage("Appointment.Date.InPast",
                "The appointment date cannot be in the past.", "Date"));
        }

        return errors;
    }

    // The chosen slot must be on the day shown in the Day box. Otherwise the save would book a slot the user
    // did not see for that day. Null means the caller has no form day to check against.
    public static IReadOnlyList<ResultMessage> ValidateSlotDay(DateTime startTime, DateOnly? shownDay)
    {
        if (shownDay is null || DateOnly.FromDateTime(startTime) == shownDay.Value)
        {
            return [];
        }

        return
        [
            new ResultMessage("Appointment.Slot.DifferentDay",
                "The chosen slot is on a different day. Choose the day, select Show slots, then pick a slot.", "Date")
        ];
    }

    public static IReadOnlyList<ResultMessage> ValidateStatusChange(string current, string requested)
    {
        if (!AppointmentStatus.IsKnown(requested))
        {
            return [new ResultMessage("Appointment.Status.Unknown", "Choose a valid status.", "Status")];
        }

        if (!AppointmentLifecycle.CanMoveManually(current, requested))
        {
            return
            [
                new ResultMessage("Appointment.Status.NotAllowed",
                    $"An appointment cannot change from {current} to {requested}.")
            ];
        }

        return [];
    }

    public static IReadOnlyList<ResultMessage> ValidateArchive(string status)
    {
        if (!AppointmentLifecycle.CanArchive(status))
        {
            return
            [
                new ResultMessage("Appointment.Archive.NotAllowed",
                    $"A {status} appointment cannot be archived. Only completed, cancelled or no-show appointments can be archived.")
            ];
        }

        return [];
    }
}
