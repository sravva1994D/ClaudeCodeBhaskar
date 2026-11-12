using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PatientManagement.Business.Interfaces;
using PatientManagement.Web.ViewModels;
using PatientManagement.Web.ViewModels.Home;

namespace PatientManagement.Web.Controllers;

// The home page (SCR-03; FR-APT-002, FR-SRC-001, FR-SRC-002): today's appointments, recent patients,
// New patient, and the top-bar quick search.
public class HomeController(
    IAppointmentService appointments,
    IPatientService patients,
    TimeProvider timeProvider) : Controller
{
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var today = DateOnly.FromDateTime(timeProvider.GetLocalNow().DateTime);
        var items = await appointments.GetDailyAsync(today, cancellationToken);
        var recent = await patients.GetRecentAsync(5, cancellationToken);

        return View(new HomeViewModel { Today = today, Appointments = items, RecentPatients = recent });
    }

    // The error page must render for signed-out users too (AllowAnonymous overrides the global policy).
    [AllowAnonymous]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
