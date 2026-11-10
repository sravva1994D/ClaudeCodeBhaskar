using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PatientManagement.Web.ViewModels;

namespace PatientManagement.Web.Controllers;

public class HomeController : Controller
{
    // Placeholder landing page for the solution skeleton. No BRD feature is shown here.
    public IActionResult Index()
    {
        return View();
    }

    // The error page must render for signed-out users too (AllowAnonymous overrides the global policy).
    [AllowAnonymous]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
