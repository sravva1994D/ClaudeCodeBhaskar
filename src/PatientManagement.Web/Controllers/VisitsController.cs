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
    [HttpGet("/Patients/{patientId:int}/Visits")]
    public async Task<IActionResult> Index(int patientId, CancellationToken cancellationToken)
    {
        var patient = await patients.GetAsync(patientId, cancellationToken);
        if (!patient.IsSuccess)
        {
            return NotFound();
        }

        // The date filter is wired to the screen in step 3 of Module 6; for now the list is unfiltered.
        var visits = await consultations.ListVisitsAsync(patientId, cancellationToken: cancellationToken);

        return View(new VisitHistoryViewModel
        {
            PatientId = patient.Value!.Id,
            PatientName = patient.Value.FullName,
            PatientCode = patient.Value.PatientCode,
            Visits = visits.Value ?? []
        });
    }

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
