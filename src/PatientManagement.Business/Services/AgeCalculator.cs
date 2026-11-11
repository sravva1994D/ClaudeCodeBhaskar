namespace PatientManagement.Business.Services;

// Age in whole years, calculated from the date of birth and the system date (BRD: Patient Management; not stored).
// The clock is injected so the result is deterministic in tests (CLAUDE.md §10).
// A 29 February birth counts its birthday on 28 February in a non-leap year (DateOnly.AddYears).
public sealed class AgeCalculator(TimeProvider timeProvider)
{
    public int CurrentAge(DateOnly dateOfBirth)
    {
        var today = DateOnly.FromDateTime(timeProvider.GetLocalNow().DateTime);
        return AgeOn(dateOfBirth, today);
    }

    public static int AgeOn(DateOnly dateOfBirth, DateOnly today)
    {
        if (dateOfBirth > today)
        {
            throw new ArgumentOutOfRangeException(nameof(dateOfBirth), "The date of birth cannot be after the date used for the age.");
        }

        var age = today.Year - dateOfBirth.Year;

        if (today < dateOfBirth.AddYears(age))
        {
            age--;
        }

        return age;
    }
}
