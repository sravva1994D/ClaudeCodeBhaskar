using PatientManagement.Business.Appointments;
using PatientManagement.Business.Validators;

namespace PatientManagement.UnitTests.Appointments;

// Unit tests for appointment rules (13-Validation-Strategy §3.3; FR-APT-001, FR-APT-003; D-M3-1 to D-M3-3).
// Test IDs follow CLAUDE.md §4.2 and §10.
public class AppointmentValidatorTests
{
    private static readonly DateOnly Today = new(2026, 11, 11);

    [Fact]
    public void FR_APT_001_a_grid_slot_today_is_accepted()
    {
        Assert.Empty(AppointmentValidator.ValidateSchedule(Today.ToDateTime(new TimeOnly(9, 0)), Today));
    }

    [Fact]
    public void FR_APT_001_a_grid_slot_on_a_future_date_is_accepted()
    {
        Assert.Empty(AppointmentValidator.ValidateSchedule(Today.AddDays(7).ToDateTime(new TimeOnly(17, 30)), Today));
    }

    [Fact]
    public void FR_APT_001_an_earlier_slot_today_is_not_rejected_as_past()
    {
        // D-M3-1: only the date is compared, so a late walk-in can still be booked for an earlier slot today.
        Assert.Empty(AppointmentValidator.ValidateSchedule(Today.ToDateTime(new TimeOnly(9, 0)), Today));
    }

    [Fact]
    public void FR_APT_001_a_date_before_today_is_rejected()
    {
        var errors = AppointmentValidator.ValidateSchedule(Today.AddDays(-1).ToDateTime(new TimeOnly(9, 0)), Today);

        Assert.Equal(["Appointment.Date.InPast"], errors.Select(error => error.Code));
    }

    [Fact]
    public void FR_APT_001_a_time_off_the_grid_is_rejected()
    {
        var errors = AppointmentValidator.ValidateSchedule(Today.ToDateTime(new TimeOnly(9, 15)), Today);

        var error = Assert.Single(errors);
        Assert.Equal("Appointment.StartTime.NotOnGrid", error.Code);
        Assert.Equal("StartTime", error.Field);
    }

    [Fact]
    public void FR_APT_001_a_past_date_and_an_off_grid_time_are_both_reported()
    {
        var errors = AppointmentValidator.ValidateSchedule(Today.AddDays(-1).ToDateTime(new TimeOnly(13, 0)), Today);

        Assert.Equal(
            ["Appointment.StartTime.NotOnGrid", "Appointment.Date.InPast"],
            errors.Select(error => error.Code));
    }

    [Fact]
    public void FR_APT_003_an_allowed_status_change_has_no_errors()
    {
        Assert.Empty(AppointmentValidator.ValidateStatusChange(AppointmentStatus.Scheduled, AppointmentStatus.CheckedIn));
    }

    [Fact]
    public void FR_APT_003_a_disallowed_status_change_names_the_current_and_requested_status()
    {
        var error = Assert.Single(
            AppointmentValidator.ValidateStatusChange(AppointmentStatus.Completed, AppointmentStatus.Scheduled));

        Assert.Equal("Appointment.Status.NotAllowed", error.Code);
        Assert.Contains(AppointmentStatus.Completed, error.Message);
        Assert.Contains(AppointmentStatus.Scheduled, error.Message);
    }

    [Fact]
    public void FR_APT_003_a_start_or_finish_status_cannot_be_set_by_hand()
    {
        var errors = AppointmentValidator.ValidateStatusChange(AppointmentStatus.CheckedIn, AppointmentStatus.InConsultation);

        Assert.Equal(["Appointment.Status.NotAllowed"], errors.Select(error => error.Code));
    }

    [Fact]
    public void FR_APT_003_an_unknown_status_is_rejected()
    {
        var error = Assert.Single(AppointmentValidator.ValidateStatusChange(AppointmentStatus.Scheduled, "Rescheduled"));

        Assert.Equal("Appointment.Status.Unknown", error.Code);
    }

    [Fact]
    public void FR_APT_003_a_completed_appointment_can_be_archived()
    {
        Assert.Empty(AppointmentValidator.ValidateArchive(AppointmentStatus.Completed));
    }

    [Fact]
    public void FR_APT_003_a_scheduled_appointment_cannot_be_archived()
    {
        Assert.Equal(["Appointment.Archive.NotAllowed"],
            AppointmentValidator.ValidateArchive(AppointmentStatus.Scheduled).Select(error => error.Code));
    }
}
