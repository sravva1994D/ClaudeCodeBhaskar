namespace PatientManagement.Business.Appointments;

// The fixed day grid of 30-minute slots (PDR-06): 09:00 to 13:00 and 17:00 to 20:00.
// The last slots start at 12:30 and 19:30. Times are local clinic time.
public static class SlotGrid
{
    public const int DurationMinutes = 30;

    public static readonly IReadOnlyList<TimeOnly> StartTimes = Build();

    public static bool IsOnGrid(DateTime startTime)
    {
        // Whole minutes only: any seconds or fractions make the time off the grid.
        if (startTime.TimeOfDay.Ticks % TimeSpan.TicksPerMinute != 0)
        {
            return false;
        }

        return StartTimes.Contains(TimeOnly.FromDateTime(startTime));
    }

    public static IReadOnlyList<DateTime> SlotsFor(DateOnly date) =>
        StartTimes.Select(time => date.ToDateTime(time)).ToArray();

    private static IReadOnlyList<TimeOnly> Build()
    {
        var starts = new List<TimeOnly>();
        AddSlots(starts, new TimeOnly(9, 0), new TimeOnly(13, 0));
        AddSlots(starts, new TimeOnly(17, 0), new TimeOnly(20, 0));
        return starts;
    }

    // Adds each slot start from 'from' up to, but not including, 'until'.
    private static void AddSlots(List<TimeOnly> starts, TimeOnly from, TimeOnly until)
    {
        for (var time = from; time < until; time = time.AddMinutes(DurationMinutes))
        {
            starts.Add(time);
        }
    }
}
