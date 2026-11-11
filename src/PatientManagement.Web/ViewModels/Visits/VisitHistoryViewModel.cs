using PatientManagement.Business.DTOs;

namespace PatientManagement.Web.ViewModels.Visits;

// The visit history for one patient (SCR-07): every visit, newest first. The date filter comes in Module 6 (FR-HIS-003).
public sealed class VisitHistoryViewModel
{
    public int PatientId { get; set; }

    public string PatientName { get; set; } = string.Empty;

    public string PatientCode { get; set; } = string.Empty;

    public IReadOnlyList<VisitSummaryDto> Visits { get; set; } = [];
}
