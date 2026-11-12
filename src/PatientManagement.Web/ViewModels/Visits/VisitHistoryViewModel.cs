using PatientManagement.Business.DTOs;

namespace PatientManagement.Web.ViewModels.Visits;

// The visit history for one patient (SCR-07): visits newest first, optionally limited to a date range (FR-HIS-003).
public sealed class VisitHistoryViewModel
{
    public int PatientId { get; set; }

    public string PatientName { get; set; } = string.Empty;

    public string PatientCode { get; set; } = string.Empty;

    public DateOnly? FromDate { get; set; }

    public DateOnly? ToDate { get; set; }

    // Set when the entered range is refused. The list is then empty and the typed dates stay in the form.
    public string? ErrorMessage { get; set; }

    public IReadOnlyList<VisitSummaryDto> Visits { get; set; } = [];

    public bool IsFiltered => FromDate is not null || ToDate is not null;
}
