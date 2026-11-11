using PatientManagement.Business.Services;

namespace PatientManagement.UnitTests.Services;

// Unit tests for the age calculation (BRD: Patient Management; FR-PAT-003 age, FR-PAT-001 AC-5).
// The clock is a fixed substitute, so the results do not depend on the real date. Test IDs follow CLAUDE.md §4.2 and §10.
public class AgeCalculatorTests
{
    private sealed class FixedClock(DateTimeOffset now) : TimeProvider
    {
        // The local zone is UTC, so the local date is the date of this fixed instant.
        public override DateTimeOffset GetUtcNow() => now.ToUniversalTime();

        public override TimeZoneInfo LocalTimeZone => TimeZoneInfo.Utc;
    }

    [Fact]
    public void FR_PAT_003_age_increases_on_the_birthday()
    {
        var dateOfBirth = new DateOnly(1990, 5, 1);

        Assert.Equal(35, AgeCalculator.AgeOn(dateOfBirth, new DateOnly(2026, 4, 30)));
        Assert.Equal(36, AgeCalculator.AgeOn(dateOfBirth, new DateOnly(2026, 5, 1)));
    }

    [Fact]
    public void FR_PAT_003_age_is_zero_on_the_day_of_birth()
    {
        var today = new DateOnly(2026, 11, 11);

        Assert.Equal(0, AgeCalculator.AgeOn(today, today));
    }

    [Fact]
    public void FR_PAT_003_leap_day_birth_counts_its_birthday_on_28_february_in_a_non_leap_year()
    {
        var dateOfBirth = new DateOnly(2000, 2, 29);

        Assert.Equal(24, AgeCalculator.AgeOn(dateOfBirth, new DateOnly(2025, 2, 27)));
        Assert.Equal(25, AgeCalculator.AgeOn(dateOfBirth, new DateOnly(2025, 2, 28)));
    }

    [Fact]
    public void FR_PAT_003_leap_day_birth_has_its_birthday_on_29_february_in_a_leap_year()
    {
        var dateOfBirth = new DateOnly(2000, 2, 29);

        Assert.Equal(27, AgeCalculator.AgeOn(dateOfBirth, new DateOnly(2028, 2, 28)));
        Assert.Equal(28, AgeCalculator.AgeOn(dateOfBirth, new DateOnly(2028, 2, 29)));
    }

    [Fact]
    public void FR_PAT_003_date_of_birth_after_the_date_is_refused()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            AgeCalculator.AgeOn(new DateOnly(2026, 11, 12), new DateOnly(2026, 11, 11)));
    }

    [Fact]
    public void FR_PAT_003_current_age_uses_the_injected_clock()
    {
        var clock = new FixedClock(new DateTimeOffset(2026, 11, 11, 9, 0, 0, TimeSpan.Zero));
        var calculator = new AgeCalculator(clock);

        Assert.Equal(36, calculator.CurrentAge(new DateOnly(1990, 11, 11)));
        Assert.Equal(35, calculator.CurrentAge(new DateOnly(1990, 11, 12)));
    }
}
