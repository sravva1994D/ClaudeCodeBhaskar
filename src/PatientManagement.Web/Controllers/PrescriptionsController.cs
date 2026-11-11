using Microsoft.AspNetCore.Mvc;
using PatientManagement.Business.Common;
using PatientManagement.Business.DTOs;
using PatientManagement.Business.Interfaces;
using PatientManagement.Business.Services;
using PatientManagement.Web.ViewModels.Consultations;
using PatientManagement.Web.ViewModels.Prescriptions;

namespace PatientManagement.Web.Controllers;

// The medicine section of the consultation screen (SCR-11, route 22; FR-RX-001). The section is on the same page as the visit,
// so a refused medicine save shows the visit text as it was typed. The controller holds no rules. PrescriptionService decides.
public class PrescriptionsController(
    IPrescriptionService prescriptions,
    IConsultationService consultations) : Controller
{
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveItems(int consultationId, ConsultationFormViewModel model, CancellationToken cancellationToken)
    {
        var lines = model.Medicines.Select(ToInput).ToList();
        var result = await prescriptions.SaveItemsAsync(consultationId, lines, cancellationToken);

        if (result.Errors.Any(error => error.Code == ConsultationService.NotFoundCode))
        {
            return NotFound();
        }

        if (!result.IsSuccess)
        {
            AddErrors(result);
            model.Id = consultationId;
            await ConsultationForms.RefreshAsync(model, consultationId, consultations, prescriptions, cancellationToken);

            return View("~/Views/Consultations/Edit.cshtml", model);
        }

        TempData["Message"] = "Medicines saved.";

        return RedirectToAction("Edit", "Consultations", new { id = consultationId });
    }

    // The print view (SCR-12, route 24). Read-only: it marks nothing.
    [HttpGet("/Prescriptions/Print/{consultationId:int}")]
    public async Task<IActionResult> Print(int consultationId, CancellationToken cancellationToken)
    {
        var result = await prescriptions.GetPrintModelAsync(consultationId, cancellationToken);
        if (result.Errors.Any(error => error.Code == ConsultationService.NotFoundCode))
        {
            return NotFound();
        }

        if (!result.IsSuccess)
        {
            TempData["Error"] = string.Join(" ", result.Errors.Select(error => error.Message));
            return RedirectToAction("Edit", "Consultations", new { id = consultationId });
        }

        return View(result.Value!);
    }

    // The Print command (route 23). Marks the prescription printed once, then shows the same view and opens the print dialog.
    // Printing again is allowed and keeps the first print time (PDR-14).
    [HttpPost("/Prescriptions/Print/{consultationId:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MarkPrinted(int consultationId, CancellationToken cancellationToken)
    {
        var result = await prescriptions.MarkPrintedAsync(consultationId, cancellationToken);
        if (result.Errors.Any(error => error.Code == ConsultationService.NotFoundCode))
        {
            return NotFound();
        }

        if (!result.IsSuccess)
        {
            TempData["Error"] = string.Join(" ", result.Errors.Select(error => error.Message));
            return RedirectToAction("Edit", "Consultations", new { id = consultationId });
        }

        ViewData["AutoPrint"] = true;

        return View("Print", result.Value!);
    }

    private static MedicineLineInput ToInput(MedicineLineFormModel line) =>
        new(line.Name, line.Dosage, line.Frequency, line.Duration, line.Instructions);

    // The service names medicine rows "Lines[n]"; the form names them "Medicines[n]". Errors about the whole list go to the summary.
    private void AddErrors<T>(Result<T> result)
    {
        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(FormKey(error.Field), error.Message);
        }
    }

    private static string FormKey(string? field) =>
        field is null || field == "Lines" ? string.Empty : field.Replace("Lines[", "Medicines[", StringComparison.Ordinal);
}
