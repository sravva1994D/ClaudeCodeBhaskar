namespace PatientManagement.Business.Common;

// The date range rule for visits (FR-HIS-003, FR-EXP-001, FR-EXP-002; D-M6-1, D-M6-8, D-M7-1). The visit list and the
// export both use it, so the two never disagree. From alone is a single day. To alone is refused, not ignored.
public static class VisitDateRange
{
    public const string OrderCode = "Visits.DateRange.Order";
    public const string FromMissingCode = "Visits.DateRange.FromMissing";

    // Returns the refusal for the range, or null when the range is valid. Both bounds null means no filter.
    public static ResultMessage? Validate(DateOnly? from, DateOnly? to)
    {
        if (from is null && to is not null)
        {
            return new ResultMessage(FromMissingCode, "Enter a From date to filter by a range.", "From");
        }

        var upper = Upper(from, to);
        if (from is not null && upper is not null && from > upper)
        {
            return new ResultMessage(OrderCode, "From date must not be after To date.", "To");
        }

        return null;
    }

    // The last day of the range. A single day is entered as From only, so the upper bound is From.
    public static DateOnly? Upper(DateOnly? from, DateOnly? to) => to ?? from;
}
