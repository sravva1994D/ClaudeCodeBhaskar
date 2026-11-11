using Microsoft.AspNetCore.Mvc;
using PatientManagement.Business.Interfaces;
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
}
