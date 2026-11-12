namespace PatientManagement.Web.ViewModels.Export;

// The export form for one patient (SCR-13; FR-EXP-001, FR-EXP-002). The range is optional and uses the visit range rule.
public sealed class ExportFormViewModel
{
    public int PatientId { get; set; }

    public string PatientName { get; set; } = string.Empty;

    public string PatientCode { get; set; } = string.Empty;

    public DateOnly? FromDate { get; set; }

    public DateOnly? ToDate { get; set; }

    // Set when the range is refused. Nothing is downloaded, and the typed dates stay in the form (D-M7-2, D-M7-6).
    public string? ErrorMessage { get; set; }
}
