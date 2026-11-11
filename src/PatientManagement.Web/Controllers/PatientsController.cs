using Microsoft.AspNetCore.Mvc;
using PatientManagement.Business.Common;
using PatientManagement.Business.DTOs;
using PatientManagement.Business.Interfaces;
using PatientManagement.Business.Services;
using PatientManagement.Web.ViewModels.Patients;

namespace PatientManagement.Web.Controllers;

// Patient screens (SCR-04 to SCR-06; Documentation/02-Planning/09-Controller-Design.md rows 7 to 12).
// The controller holds no rules. PatientService decides.
public class PatientsController(IPatientService patients) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Search(string? q, CancellationToken cancellationToken)
    {
        var model = new PatientSearchViewModel { Term = q?.Trim() ?? string.Empty };

        // A page opened without a search shows the empty form, with no error. A submitted search is checked,
        // even when it is blank: MVC turns a blank value into null, so the presence of the key is what matters.
        if (!Request.Query.ContainsKey("q"))
        {
            return View(model);
        }

        var result = await patients.SearchAsync(q ?? string.Empty, cancellationToken);

        if (!result.IsSuccess)
        {
            // The search box is not a form field, so its error goes in the summary, not beside a field.
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Message);
            }

            return View(model);
        }

        model.Searched = true;
        model.Results = result.Value!;

        return View(model);
    }

    [HttpGet]
    public IActionResult Create() => View(new PatientFormViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(PatientFormViewModel model, CancellationToken cancellationToken)
    {
        var result = await patients.RegisterAsync(ToInput(model), model.ConfirmWarnings, cancellationToken);

        if (result.RequiresConfirmation)
        {
            return View(WithWarnings(model, result));
        }

        if (!result.IsSuccess)
        {
            AddErrors(result);
            return View(model);
        }

        return RedirectToAction(nameof(Details), new { id = result.Value!.Id });
    }

    [HttpGet]
    public async Task<IActionResult> Details(int id, CancellationToken cancellationToken)
    {
        var result = await patients.GetAsync(id, cancellationToken);

        return result.IsSuccess ? View(result.Value) : NotFound();
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken)
    {
        var result = await patients.GetAsync(id, cancellationToken);

        return result.IsSuccess ? View(ToForm(result.Value!)) : NotFound();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, PatientFormViewModel model, CancellationToken cancellationToken)
    {
        model.Id = id;
        var result = await patients.UpdateAsync(id, ToInput(model), model.ConfirmWarnings, cancellationToken);

        if (result.Errors.Any(error => error.Code == PatientService.NotFoundCode))
        {
            return NotFound();
        }

        if (result.RequiresConfirmation)
        {
            return View(WithWarnings(model, result));
        }

        if (!result.IsSuccess)
        {
            AddErrors(result);
            return View(model);
        }

        return RedirectToAction(nameof(Details), new { id });
    }

    private static PatientInput ToInput(PatientFormViewModel model) => new(
        model.FullName,
        model.DateOfBirth,
        model.Gender,
        model.Phone,
        model.Email,
        model.Address);

    private static PatientFormViewModel ToForm(PatientDto patient) => new()
    {
        Id = patient.Id,
        PatientCode = patient.PatientCode,
        FullName = patient.FullName,
        DateOfBirth = patient.DateOfBirth,
        Gender = patient.Gender,
        Phone = patient.Phone,
        Email = patient.Email,
        Address = patient.Address
    };

    // The form is shown again with the warnings. The user changes nothing or presses "Confirm and save".
    private static PatientFormViewModel WithWarnings(PatientFormViewModel model, Result<PatientDto> result)
    {
        model.ConfirmWarnings = false;
        model.Warnings = result.Warnings.Select(warning => warning.Message).ToList();
        return model;
    }

    private void AddErrors<T>(Result<T> result)
    {
        foreach (var error in result.Errors)
        {
            // Errors with a field name go next to that field; the rest appear in the summary.
            ModelState.AddModelError(error.Field ?? string.Empty, error.Message);
        }
    }
}
