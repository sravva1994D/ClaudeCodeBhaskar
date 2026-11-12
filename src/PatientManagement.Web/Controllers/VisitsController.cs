using Microsoft.AspNetCore.Mvc;
using PatientManagement.Business.Interfaces;
using PatientManagement.Web.ViewModels.Visits;

namespace PatientManagement.Web.Controllers;

// Visit history and visit detail (SCR-07, SCR-08; Documentation/02-Planning/09-Controller-Design.md rows 13 and 14).
// The controller holds no rules. The services decide.
public class VisitsController(
    IConsultationService consultations,
    IPrescriptionService prescriptions,
    IPatientService patients) : Controller
{
    // The range comes from the query string as yyyy-MM-dd. Both ends are optional (FR-HIS-003).
    [HttpGet("/Patients/{patientId:int}/Visits")]
    public async Task<IActionResult> Index(
        int patientId,
        [FromQuery(Name = "from")] DateOnly? fromDate,
        [FromQuery(Name = "to")] DateOnly? toDate,
        CancellationToken cancellationToken)
    {
        var patient = await patients.GetAsync(patientId, cancellationToken);
        if (!patient.IsSuccess)
        {
            return NotFound();
        }

        // A date that is not a real date (for example 2026-02-30) is refused, not dropped. Dropping it would show an
        // unfiltered list as if the filter had been applied.
        if (HasMalformedDate())
        {
            return View(new VisitHistoryViewModel
            {
                PatientId = patient.Value!.Id,
                PatientName = patient.Value.FullName,
                PatientCode = patient.Value.PatientCode,
                ErrorMessage = "Enter each date as a real date, for example 2026-11-03."
            });
        }

        var visits = await consultations.ListVisitsAsync(patientId, fromDate, toDate, cancellationToken);

        return View(new VisitHistoryViewModel
        {
            PatientId = patient.Value!.Id,
            PatientName = patient.Value.FullName,
            PatientCode = patient.Value.PatientCode,
            FromDate = fromDate,
            ToDate = toDate,
            ErrorMessage = visits.Errors.FirstOrDefault()?.Message,
            Visits = visits.Value ?? []
        });
    }

    // Model binding records an error for a non-empty value that is not a date. The two keys are the query names and the parameter names.
    private bool HasMalformedDate() =>
        ModelState.Any(entry => (entry.Key is "from" or "to" or "fromDate" or "toDate") && entry.Value?.Errors.Count > 0);

    [HttpGet]
    public async Task<IActionResult> Details(int id, CancellationToken cancellationToken)
    {
        var visit = await consultations.GetAsync(id, cancellationToken);
        if (!visit.IsSuccess)
        {
            return NotFound();
        }

        var prescription = await prescriptions.GetAsync(id, cancellationToken);

        return View(new VisitDetailViewModel { Visit = visit.Value!, Prescription = prescription });
    }
}
