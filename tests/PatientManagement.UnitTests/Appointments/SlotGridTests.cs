using PatientManagement.Business.Appointments;

namespace PatientManagement.UnitTests.Appointments;

// Unit tests for the slot grid (PDR-06: 09:00 to 13:00 and 17:00 to 20:00, 30-minute slots). Test IDs follow CLAUDE.md §4.2 and §10.
public class SlotGridTests
{
    private static readonly DateOnly Day = new(2026, 12, 1);

    [Fact]
    public void FR_APT_001_grid_has_fourteen_start_times_from_0900_to_1930()
    {
        Assert.Equal(14, SlotGrid.StartTimes.Count);
        Assert.Equal(new TimeOnly(9, 0), SlotGrid.StartTimes[0]);
        Assert.Equal(new TimeOnly(12, 30), SlotGrid.StartTimes[7]);
        Assert.Equal(new TimeOnly(17, 0), SlotGrid.StartTimes[8]);
        Assert.Equal(new TimeOnly(19, 30), SlotGrid.StartTimes[^1]);
    }

    [Fact]
    public void FR_APT_001_the_lunch_hour_is_not_on_the_grid()
    {
        Assert.DoesNotContain(new TimeOnly(13, 0), SlotGrid.StartTimes);
        Assert.DoesNotContain(new TimeOnly(16, 30), SlotGrid.StartTimes);
    }

    [Theory]
    [InlineData(9, 0)]
    [InlineData(9, 30)]
    [InlineData(12, 30)]
    [InlineData(17, 0)]
    [InlineData(19, 30)]
    public void FR_APT_001_a_grid_start_time_is_accepted(int hour, int minute)
    {
        Assert.True(SlotGrid.IsOnGrid(Day.ToDateTime(new TimeOnly(hour, minute))));
    }

    [Theory]
    [InlineData(8, 30)]
    [InlineData(9, 15)]
    [InlineData(12, 45)]
    [InlineData(13, 0)]
    [InlineData(16, 30)]
    [InlineData(19, 45)]
    [InlineData(20, 0)]
    public void FR_APT_001_a_time_off_the_grid_is_refused(int hour, int minute)
    {
        Assert.False(SlotGrid.IsOnGrid(Day.ToDateTime(new TimeOnly(hour, minute))));
    }

    [Fact]
    public void FR_APT_001_seconds_make_a_time_off_the_grid()
    {
        Assert.False(SlotGrid.IsOnGrid(new DateTime(2026, 12, 1, 9, 0, 30)));
    }

    [Fact]
    public void FR_APT_001_fractions_of_a_second_make_a_time_off_the_grid()
    {
        Assert.False(SlotGrid.IsOnGrid(new DateTime(2026, 12, 1, 9, 0, 0, 1)));
    }

    [Fact]
    public void FR_APT_001_slots_for_a_day_are_the_fourteen_grid_times_on_that_date()
    {
        var slots = SlotGrid.SlotsFor(Day);

        Assert.Equal(14, slots.Count);
        Assert.All(slots, slot => Assert.Equal(Day, DateOnly.FromDateTime(slot)));
        Assert.All(slots, slot => Assert.True(SlotGrid.IsOnGrid(slot)));
    }
}
