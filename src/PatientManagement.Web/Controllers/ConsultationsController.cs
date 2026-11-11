using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using PatientManagement.Business.Common;
using PatientManagement.Business.DTOs;
using PatientManagement.Business.Interfaces;
using PatientManagement.Business.Services;
using PatientManagement.Business.Validators;
using PatientManagement.Web.ViewModels.Consultations;

namespace PatientManagement.Web.Controllers;

// The consultation screen and its actions (SCR-11; Documentation/02-Planning/09-Controller-Design.md rows 19, 20, 21, 31).
// The controller holds no rules. ConsultationService decides.
public class ConsultationsController(
    IConsultationService consultations,
    IPrescriptionService prescriptions,
    TimeProvider timeProvider) : Controller
{
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Start(int patientId, int? appointmentId, DateOnly? date, CancellationToken cancellationToken)
    {
        var result = await consultations.StartAsync(patientId, appointmentId, cancellationToken);

        if (result.IsSuccess)
        {
            return RedirectToAction(nameof(Edit), new { id = result.Value!.Id });
        }

        TempData["Error"] = Message(result);

        // Back to where the user started: the list day for an appointment, the profile for a walk-in.
        return appointmentId is null
            ? RedirectToAction("Details", "Patients", new { id = patientId })
            : RedirectToAction("Daily", "Appointments", new { date = (date ?? Today()).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) });
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken)
    {
        var result = await consultations.GetAsync(id, cancellationToken);
        if (!result.IsSuccess)
        {
            return NotFound();
        }

        var model = ConsultationForms.FromDto(result.Value!);
        await ConsultationForms.ApplyPrescriptionAsync(model, id, prescriptions, cancellationToken);

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, ConsultationFormViewModel model, CancellationToken cancellationToken)
    {
        var result = await consultations.SaveAsync(id, ToInput(model), model.ConfirmWarnings, cancellationToken);

        if (result.Errors.Any(error => error.Code == ConsultationService.NotFoundCode))
        {
            return NotFound();
        }

        if (result.RequiresConfirmation)
        {
            // Nothing is saved. The form comes back with the warnings and "Confirm and save" (AD-11).
            model.ConfirmWarnings = false;
            model.Warnings = result.Warnings.Select(warning => warning.Message).ToList();
            return View(await WithHeaderAsync(model, id, cancellationToken));
        }

        if (!result.IsSuccess)
        {
            AddErrors(result);
            return View(await WithHeaderAsync(model, id, cancellationToken));
        }

        TempData["Message"] = Saved(result.Warnings);

        return RedirectToAction(nameof(Edit), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Finish(int id, CancellationToken cancellationToken)
    {
        var result = await consultations.FinishAsync(id, cancellationToken);

        if (result.Errors.Any(error => error.Code == ConsultationService.NotFoundCode))
        {
            return NotFound();
        }

        if (result.IsSuccess)
        {
            TempData["Message"] = "Visit finished.";
        }
        else
        {
            TempData["Error"] = Message(result);
        }

        return RedirectToAction(nameof(Edit), new { id });
    }

    private static ConsultationSaveInput ToInput(ConsultationFormViewModel model) => new(
        model.Complaints,
        model.Diagnosis,
        new VitalsInput(model.TemperatureC, model.SystolicMmHg, model.DiastolicMmHg, model.PulseBpm));

    // Re-reads the header and the prescription state so the page is shown again after a refused save. The entered values stay as they were.
    private async Task<ConsultationFormViewModel> WithHeaderAsync(
        ConsultationFormViewModel model, int id, CancellationToken cancellationToken)
    {
        await ConsultationForms.RefreshAsync(model, id, consultations, prescriptions, cancellationToken);

        return model;
    }

    // The saved message names any notice (no vitals) or accepted warning, so the user sees what was saved.
    private static string Saved(IEnumerable<ResultMessage> notesAndWarnings)
    {
        var notes = notesAndWarnings.Select(note => note.Message).ToList();

        return notes.Count == 0 ? "Visit saved." : $"Visit saved. {string.Join(" ", notes)}";
    }

    private static string Message<T>(Result<T> result) =>
        string.Join(" ", result.Errors.Select(error => error.Message));

    private DateOnly Today() => DateOnly.FromDateTime(timeProvider.GetLocalNow().DateTime);

    // Errors with a field name go next to that field; the rest appear in the summary.
    private void AddErrors<T>(Result<T> result)
    {
        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(error.Field ?? string.Empty, error.Message);
        }
    }
}
