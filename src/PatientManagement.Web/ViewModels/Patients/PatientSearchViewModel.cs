using PatientManagement.Business.DTOs;

namespace PatientManagement.Web.ViewModels.Patients;

public class PatientSearchViewModel
{
    public string Term { get; set; } = string.Empty;

    // True once a search has been submitted, so the page can say "no match" only after a real search.
    public bool Searched { get; set; }

    public IReadOnlyList<PatientSummaryDto> Results { get; set; } = [];
}
