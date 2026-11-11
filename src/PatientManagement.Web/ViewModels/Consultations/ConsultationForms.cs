using PatientManagement.Business.DTOs;
using PatientManagement.Business.Interfaces;
using PatientManagement.Web.ViewModels.Prescriptions;

namespace PatientManagement.Web.ViewModels.Consultations;

// Builds and refreshes the consultation screen model (SCR-11), so the visit and the medicine actions show the same state.
public static class ConsultationForms
{
    public static ConsultationFormViewModel FromDto(ConsultationDto visit)
    {
        var model = new ConsultationFormViewModel
        {
            Complaints = visit.Complaints,
            Diagnosis = visit.Diagnosis,
            TemperatureC = visit.Vitals?.TemperatureC,
            SystolicMmHg = visit.Vitals?.SystolicMmHg,
            DiastolicMmHg = visit.Vitals?.DiastolicMmHg,
            PulseBpm = visit.Vitals?.PulseBpm
        };
        ApplyHeader(model, visit);

        return model;
    }

    // The header is display only. It is read again after a refused save, so the page is never shown with stale details.
    public static void ApplyHeader(ConsultationFormViewModel model, ConsultationDto visit)
    {
        model.Id = visit.Id;
        model.PatientId = visit.PatientId;
        model.PatientName = visit.PatientName;
        model.PatientCode = visit.PatientCode;
        model.AppointmentId = visit.AppointmentId;
        model.VisitDateTime = visit.VisitDateTime;
        model.IsFinished = visit.IsFinished;
    }

    // Once printed, the page shows the saved medicines. Otherwise the typed medicines are kept, so a refused save
    // does not lose them; the saved medicines show only when nothing was typed (the GET of the page).
    public static async Task ApplyPrescriptionAsync(
        ConsultationFormViewModel model, int consultationId, IPrescriptionService prescriptions, CancellationToken cancellationToken)
    {
        var saved = await prescriptions.GetAsync(consultationId, cancellationToken);

        model.PrescriptionIsPrinted = saved?.IsPrinted ?? false;
        if (model.PrescriptionIsPrinted || model.Medicines.Count == 0)
        {
            model.Medicines = saved?.Lines.Select(FromLine).ToList() ?? [];
        }
    }

    public static async Task RefreshAsync(
        ConsultationFormViewModel model,
        int consultationId,
        IConsultationService consultations,
        IPrescriptionService prescriptions,
        CancellationToken cancellationToken)
    {
        var visit = await consultations.GetAsync(consultationId, cancellationToken);
        if (visit.IsSuccess)
        {
            ApplyHeader(model, visit.Value!);
        }

        await ApplyPrescriptionAsync(model, consultationId, prescriptions, cancellationToken);
    }

    private static MedicineLineFormModel FromLine(MedicineLineDto line) => new()
    {
        Name = line.Name,
        Dosage = line.Dosage,
        Frequency = line.Frequency,
        Duration = line.Duration,
        Instructions = line.Instructions
    };
}
