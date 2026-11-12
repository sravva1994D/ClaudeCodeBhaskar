using Microsoft.AspNetCore.Mvc;
using PatientManagement.Business.Interfaces;
using PatientManagement.Business.Services;
using PatientManagement.Web.ViewModels.Export;

namespace PatientManagement.Web.Controllers;

// Export for one patient (SCR-13; Documentation/02-Planning/09-Controller-Design.md rows 25 and 26).
// The controller holds no rules. The service decides. A refused range is shown on the form, not as a blank file (D-M7-6).
public class ExportController(IExportService exports) : Controller
{
    private const string MalformedDateMessage = "Enter each date as a real date, for example 2026-11-03.";

    [HttpGet("/Export/Patient/{patientId:int}")]
    public Task<IActionResult> Patient(
        int patientId,
        [FromQuery(Name = "from")] DateOnly? fromDate,
        [FromQuery(Name = "to")] DateOnly? toDate,
        CancellationToken cancellationToken)
    {
        // A date that is not a real date is refused, not dropped (D-M7-2).
        var error = HasMalformedDate() ? MalformedDateMessage : null;

        return FormAsync(patientId, error is null ? fromDate : null, error is null ? toDate : null, error, cancellationToken);
    }

    [HttpGet("/Export/Patient/{patientId:int}/Csv")]
    public async Task<IActionResult> PatientCsv(
        int patientId,
        [FromQuery(Name = "from")] DateOnly? fromDate,
        [FromQuery(Name = "to")] DateOnly? toDate,
        CancellationToken cancellationToken)
    {
        if (HasMalformedDate())
        {
            return await FormAsync(patientId, null, null, MalformedDateMessage, cancellationToken);
        }

        var file = await exports.BuildCsvAsync(patientId, fromDate, toDate, cancellationToken);
        if (file.IsSuccess)
        {
            return File(file.Value!.Content, "text/csv; charset=utf-8", file.Value.FileName);
        }

        if (file.Errors.Any(error => error.Code == ExportService.PatientNotFoundCode))
        {
            return NotFound();
        }

        return await FormAsync(patientId, fromDate, toDate, file.Errors[0].Message, cancellationToken);
    }

    // The printable view (FR-EXP-002). Printing it in the browser, or saving it as PDF, changes nothing in the system (PDR-18).
    [HttpGet("/Export/Patient/{patientId:int}/Print")]
    public async Task<IActionResult> PatientPrint(
        int patientId,
        [FromQuery(Name = "from")] DateOnly? fromDate,
        [FromQuery(Name = "to")] DateOnly? toDate,
        CancellationToken cancellationToken)
    {
        if (HasMalformedDate())
        {
            return await FormAsync(patientId, null, null, MalformedDateMessage, cancellationToken);
        }

        var print = await exports.BuildPrintViewAsync(patientId, fromDate, toDate, cancellationToken);
        if (print.IsSuccess)
        {
            return View("Print", print.Value);
        }

        if (print.Errors.Any(error => error.Code == ExportService.PatientNotFoundCode))
        {
            return NotFound();
        }

        return await FormAsync(patientId, fromDate, toDate, print.Errors[0].Message, cancellationToken);
    }

    // The form, with the patient's name and code. An unknown patient is not found.
    private async Task<IActionResult> FormAsync(
        int patientId, DateOnly? fromDate, DateOnly? toDate, string? errorMessage, CancellationToken cancellationToken)
    {
        var options = await exports.GetOptionsAsync(patientId, cancellationToken);
        if (!options.IsSuccess)
        {
            return NotFound();
        }

        return View("Patient", new ExportFormViewModel
        {
            PatientId = options.Value!.PatientId,
            PatientName = options.Value.FullName,
            PatientCode = options.Value.PatientCode,
            FromDate = fromDate,
            ToDate = toDate,
            ErrorMessage = errorMessage
        });
    }

    // Model binding records an error for a non-empty value that is not a date (the same check as the visit history).
    private bool HasMalformedDate() =>
        ModelState.Any(entry => (entry.Key is "from" or "to" or "fromDate" or "toDate") && entry.Value?.Errors.Count > 0);
}
