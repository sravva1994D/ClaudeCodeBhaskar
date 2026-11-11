using Microsoft.AspNetCore.Mvc;
using PatientManagement.Business.Interfaces;
using PatientManagement.Web.ViewModels.Visits;

namespace PatientManagement.Web.Controllers;

// Visit history and visit detail (SCR-07, SCR-08; Documentation/02-Planning/09-Controller-Design.md rows 13 and 14).
// The controller holds no rules. The services decide.
public class VisitsController(IConsultationService consultations, IPatientService patients) : Controller
{
    [HttpGet("/Patients/{patientId:int}/Visits")]
    public async Task<IActionResult> Index(int patientId, CancellationToken cancellationToken)
    {
        var patient = await patients.GetAsync(patientId, cancellationToken);
        if (!patient.IsSuccess)
        {
            return NotFound();
        }

        var visits = await consultations.ListVisitsAsync(patientId, cancellationToken);

        return View(new VisitHistoryViewModel
        {
            PatientId = patient.Value!.Id,
            PatientName = patient.Value.FullName,
            PatientCode = patient.Value.PatientCode,
            Visits = visits
        });
    }

    [HttpGet]
    public async Task<IActionResult> Details(int id, CancellationToken cancellationToken)
    {
        var visit = await consultations.GetAsync(id, cancellationToken);

        return visit.IsSuccess ? View(visit.Value) : NotFound();
    }
}
