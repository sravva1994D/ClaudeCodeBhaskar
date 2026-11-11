using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using PatientManagement.Business.Common;
using PatientManagement.Business.DTOs;
using PatientManagement.Business.Interfaces;
using PatientManagement.Web.ViewModels.Appointments;

namespace PatientManagement.Web.Controllers;

// Appointment screens (SCR-09, SCR-10; Documentation/02-Planning/09-Controller-Design.md rows 15 and 16).
// The controller holds no rules. AppointmentService decides.
public class AppointmentsController(
    IAppointmentService appointments,
    IPatientService patients,
    TimeProvider timeProvider) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Create(int patientId, DateOnly? date, CancellationToken cancellationToken)
    {
        var patient = await patients.GetAsync(patientId, cancellationToken);
        if (!patient.IsSuccess)
        {
            return NotFound();
        }

        var day = date ?? Today();

        return View(await BuildFormAsync(patient.Value!, day, startTime: null, cancellationToken));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(AppointmentFormViewModel model, CancellationToken cancellationToken)
    {
        var patient = await patients.GetAsync(model.PatientId, cancellationToken);
        if (!patient.IsSuccess)
        {
            return NotFound();
        }

        var day = model.Date ?? Today();

        // Choosing a slot is a form requirement: nothing is posted until a radio button is picked.
        if (model.StartTime is null)
        {
            ModelState.AddModelError(nameof(model.StartTime), "Choose a slot.");
            return View(await BuildFormAsync(patient.Value!, day, startTime: null, cancellationToken));
        }

        var result = await appointments.ScheduleAsync(
            new AppointmentCreateDto(model.PatientId, model.StartTime.Value), cancellationToken);

        if (!result.IsSuccess)
        {
            AddErrors(result);
            return View(await BuildFormAsync(patient.Value!, day, model.StartTime, cancellationToken));
        }

        TempData["Message"] =
            $"Appointment booked for {model.StartTime.Value:yyyy-MM-dd} at {model.StartTime.Value:HH:mm}.";

        // The daily list (SCR-10) arrives in step 6; until then the profile confirms the booking.
        return RedirectToAction("Details", "Patients", new { id = model.PatientId });
    }

    private async Task<AppointmentFormViewModel> BuildFormAsync(
        PatientDto patient, DateOnly day, DateTime? startTime, CancellationToken cancellationToken)
    {
        var slots = await appointments.GetSlotsAsync(day, cancellationToken);

        return new AppointmentFormViewModel
        {
            PatientId = patient.Id,
            PatientName = patient.FullName,
            PatientCode = patient.PatientCode,
            Date = day,
            StartTime = startTime,
            Slots = slots
                .Select(slot => new SlotViewModel(
                    slot.StartTime,
                    slot.StartTime.ToString("HH:mm", CultureInfo.InvariantCulture),
                    slot.IsTaken))
                .ToList()
        };
    }

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
