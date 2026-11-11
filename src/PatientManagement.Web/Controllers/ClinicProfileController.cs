using Microsoft.AspNetCore.Mvc;
using PatientManagement.Business.Common;
using PatientManagement.Business.Interfaces;
using PatientManagement.Web.ViewModels.ClinicProfile;

namespace PatientManagement.Web.Controllers;

// The clinic header printed on prescriptions (SCR-14; Documentation/02-Planning/09-Controller-Design.md rows 28, 29; FR-RX-002, PDR-15).
// The controller holds no rules. ClinicProfileService decides.
public class ClinicProfileController(IClinicProfileService clinicProfile) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Edit(CancellationToken cancellationToken)
    {
        var profile = await clinicProfile.GetAsync(cancellationToken);

        return View(ClinicProfileFormViewModel.From(profile));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(ClinicProfileFormViewModel model, CancellationToken cancellationToken)
    {
        var result = await clinicProfile.UpdateAsync(model.ToInput(), cancellationToken);

        if (!result.IsSuccess)
        {
            AddErrors(result);
            return View(model);
        }

        TempData["Message"] = "Clinic details saved.";

        return RedirectToAction(nameof(Edit));
    }

    // Errors name the field they belong to, so each message appears next to its input.
    private void AddErrors<T>(Result<T> result)
    {
        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(error.Field ?? string.Empty, error.Message);
        }
    }
}
