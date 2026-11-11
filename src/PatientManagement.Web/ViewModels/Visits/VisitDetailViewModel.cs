using PatientManagement.Business.DTOs;

namespace PatientManagement.Web.ViewModels.Visits;

// One visit (SCR-08) with its prescription, if one was saved (FR-HIS-002). The medicines are read-only here.
public sealed class VisitDetailViewModel
{
    public ConsultationDto Visit { get; set; } = null!;

    public PrescriptionDto? Prescription { get; set; }
}
