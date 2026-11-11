using PatientManagement.Business.Appointments;

namespace PatientManagement.UnitTests.Appointments;

// Unit tests for the status transition table (19-Appointment-Status-Lifecycle §2; FR-APT-003; PDR-08, PDR-27).
// The expected moves are written out here, not read from the rule, so the test checks the rule against the document.
// Test IDs follow CLAUDE.md §4.2 and §10.
public class AppointmentLifecycleTests
{
    // Moves a user can make by hand in M3 (decision D-M3-2).
    private static readonly (string From, string To)[] ManualMoves =
    [
        (AppointmentStatus.Scheduled, AppointmentStatus.CheckedIn),
        (AppointmentStatus.Scheduled, AppointmentStatus.Cancelled),
        (AppointmentStatus.Scheduled, AppointmentStatus.NoShow),
        (AppointmentStatus.CheckedIn, AppointmentStatus.Cancelled)
    ];

    // Moves made only by the consultation Start and Finish actions (M4; PDR-27).
    private static readonly (string From, string To)[] StartAndFinishMoves =
    [
        (AppointmentStatus.CheckedIn, AppointmentStatus.InConsultation),
        (AppointmentStatus.InConsultation, AppointmentStatus.Completed)
    ];

    public static TheoryData<string, string> EveryPairOfStatuses()
    {
        var data = new TheoryData<string, string>();
        foreach (var from in AppointmentStatus.All)
        {
            foreach (var to in AppointmentStatus.All)
            {
                data.Add(from, to);
            }
        }

        return data;
    }

    public static TheoryData<string> TerminalStatuses() =>
        new(AppointmentStatus.Completed, AppointmentStatus.Cancelled, AppointmentStatus.NoShow);

    [Theory]
    [MemberData(nameof(EveryPairOfStatuses))]
    public void FR_APT_003_manual_change_is_allowed_only_for_the_listed_moves(string from, string to)
    {
        var expected = ManualMoves.Contains((from, to));

        Assert.Equal(expected, AppointmentLifecycle.CanMoveManually(from, to));
    }

    [Theory]
    [MemberData(nameof(EveryPairOfStatuses))]
    public void FR_APT_003_lifecycle_allows_exactly_the_listed_moves(string from, string to)
    {
        var expected = ManualMoves.Contains((from, to)) || StartAndFinishMoves.Contains((from, to));

        Assert.Equal(expected, AppointmentLifecycle.CanMove(from, to));
    }

    [Theory]
    [InlineData(AppointmentStatus.Scheduled, AppointmentStatus.CheckedIn)]
    [InlineData(AppointmentStatus.Scheduled, AppointmentStatus.Cancelled)]
    [InlineData(AppointmentStatus.Scheduled, AppointmentStatus.NoShow)]
    [InlineData(AppointmentStatus.CheckedIn, AppointmentStatus.Cancelled)]
    public void FR_APT_003_each_allowed_manual_move_is_accepted(string from, string to)
    {
        Assert.True(AppointmentLifecycle.CanMoveManually(from, to));
    }

    [Theory]
    [InlineData(AppointmentStatus.CheckedIn, AppointmentStatus.InConsultation)]
    [InlineData(AppointmentStatus.InConsultation, AppointmentStatus.Completed)]
    public void FR_APT_003_start_and_finish_moves_are_allowed_by_the_lifecycle_but_not_by_hand(string from, string to)
    {
        Assert.True(AppointmentLifecycle.CanMove(from, to));
        Assert.False(AppointmentLifecycle.CanMoveManually(from, to));
    }

    [Theory]
    [InlineData(AppointmentStatus.Scheduled, AppointmentStatus.Completed)]
    [InlineData(AppointmentStatus.Scheduled, AppointmentStatus.InConsultation)]
    [InlineData(AppointmentStatus.CheckedIn, AppointmentStatus.Completed)]
    [InlineData(AppointmentStatus.CheckedIn, AppointmentStatus.Scheduled)]
    [InlineData(AppointmentStatus.InConsultation, AppointmentStatus.CheckedIn)]
    [InlineData(AppointmentStatus.Completed, AppointmentStatus.InConsultation)]
    public void FR_APT_003_skipped_and_reversed_moves_are_blocked(string from, string to)
    {
        Assert.False(AppointmentLifecycle.CanMove(from, to));
        Assert.False(AppointmentLifecycle.CanMoveManually(from, to));
    }

    [Theory]
    [InlineData(AppointmentStatus.Cancelled, AppointmentStatus.Scheduled)]
    [InlineData(AppointmentStatus.NoShow, AppointmentStatus.Scheduled)]
    [InlineData(AppointmentStatus.Cancelled, AppointmentStatus.CheckedIn)]
    [InlineData(AppointmentStatus.NoShow, AppointmentStatus.CheckedIn)]
    [InlineData(AppointmentStatus.Completed, AppointmentStatus.Completed)]
    public void FR_APT_003_reversal_out_of_a_terminal_status_is_blocked(string from, string to)
    {
        Assert.False(AppointmentLifecycle.CanMove(from, to));
    }

    [Theory]
    [MemberData(nameof(TerminalStatuses))]
    public void FR_APT_003_terminal_statuses_allow_no_move(string from)
    {
        foreach (var to in AppointmentStatus.All)
        {
            Assert.False(AppointmentLifecycle.CanMove(from, to));
        }
    }

    [Theory]
    [InlineData(AppointmentStatus.Scheduled, true)]
    [InlineData(AppointmentStatus.CheckedIn, true)]
    [InlineData(AppointmentStatus.InConsultation, true)]
    [InlineData(AppointmentStatus.Completed, true)]
    [InlineData(AppointmentStatus.Cancelled, false)]
    [InlineData(AppointmentStatus.NoShow, false)]
    public void FR_APT_001_only_live_and_completed_statuses_hold_the_slot(string status, bool holds)
    {
        Assert.Equal(holds, AppointmentStatus.HoldsSlot(status));
    }

    [Fact]
    public void FR_APT_003_a_scheduled_appointment_offers_the_three_manual_moves()
    {
        Assert.Equal(
            [AppointmentStatus.CheckedIn, AppointmentStatus.Cancelled, AppointmentStatus.NoShow],
            AppointmentLifecycle.ManualNextStatuses(AppointmentStatus.Scheduled));
    }

    [Fact]
    public void FR_APT_003_a_completed_appointment_offers_no_manual_moves()
    {
        Assert.Empty(AppointmentLifecycle.ManualNextStatuses(AppointmentStatus.Completed));
    }

    [Theory]
    [InlineData(AppointmentStatus.Completed, true)]
    [InlineData(AppointmentStatus.Cancelled, true)]
    [InlineData(AppointmentStatus.NoShow, true)]
    [InlineData(AppointmentStatus.Scheduled, false)]
    [InlineData(AppointmentStatus.CheckedIn, false)]
    [InlineData(AppointmentStatus.InConsultation, false)]
    public void FR_APT_003_only_finished_or_released_appointments_can_be_archived(string status, bool allowed)
    {
        Assert.Equal(allowed, AppointmentLifecycle.CanArchive(status));
    }

    [Theory]
    [InlineData("Rescheduled", false)]
    [InlineData("", false)]
    [InlineData("completed", false)]
    [InlineData("No-show", true)]
    public void FR_APT_003_only_the_six_status_values_are_known(string value, bool known)
    {
        Assert.Equal(known, AppointmentStatus.IsKnown(value));
    }
}
