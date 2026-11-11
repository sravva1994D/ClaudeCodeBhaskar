namespace PatientManagement.Web.ViewModels.Consultations;

// The consultation screen (SCR-11). Form fields are nullable so the service's messages show, not MVC's required messages
// (13-Validation-Strategy §5). The header fields are display values and are not posted.
public sealed class ConsultationFormViewModel
{
    public int Id { get; set; }

    public int PatientId { get; set; }

    public string? PatientName { get; set; }

    public string? PatientCode { get; set; }

    public int? AppointmentId { get; set; }

    public DateTime? VisitDateTime { get; set; }

    public bool IsFinished { get; set; }

    public string? Complaints { get; set; }

    public string? Diagnosis { get; set; }

    public decimal? TemperatureC { get; set; }

    public int? SystolicMmHg { get; set; }

    public int? DiastolicMmHg { get; set; }

    public int? PulseBpm { get; set; }

    // Set only by the "Confirm and save" button (AD-11).
    public bool ConfirmWarnings { get; set; }

    public IReadOnlyList<string> Warnings { get; set; } = [];
}
