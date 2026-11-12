using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using PatientManagement.Business.Common;
using PatientManagement.Business.DTOs;
using PatientManagement.Business.Interfaces;
using PatientManagement.Business.Services;
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

        // The slot must be on the day shown in the Day box. Otherwise the user changed the day without
        // reloading the slots, and the save would book a slot the user did not see for that day.
        if (DateOnly.FromDateTime(model.StartTime.Value) != day)
        {
            ModelState.AddModelError(nameof(model.Date),
                "The chosen slot is on a different day. Choose the day, select Show slots, then pick a slot.");
            return View(await BuildFormAsync(patient.Value!, day, model.StartTime, cancellationToken));
        }

        var result = await appointments.ScheduleAsync(
            new AppointmentCreateDto(model.PatientId, model.StartTime.Value), cancellationToken);

        if (!result.IsSuccess)
        {
            AddErrors(result);
            return View(await BuildFormAsync(patient.Value!, day, model.StartTime, cancellationToken));
        }

        TempData["Message"] =
            $"Appointment booked for {model.StartTime.Value:dd-MMM-yyyy} at {model.StartTime.Value:HH:mm}.";

        return RedirectToAction("Details", "Patients", new { id = model.PatientId });
    }

    // The daily list (SCR-10; FR-APT-002). Archived appointments are not returned by the service.
    [HttpGet]
    public async Task<IActionResult> Daily(DateOnly? date, CancellationToken cancellationToken)
    {
        var day = date ?? Today();
        var items = await appointments.GetDailyAsync(day, cancellationToken);

        return View(new DailyAppointmentsViewModel { Date = day, Items = items });
    }

    // Status changes are posted from the list. The result goes back to the list as a message (FR-APT-003).
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateStatus(int id, string status, DateOnly? date, CancellationToken cancellationToken)
    {
        var result = await appointments.UpdateStatusAsync(id, status, cancellationToken);

        if (result.Errors.Any(error => error.Code == AppointmentService.NotFoundCode))
        {
            return NotFound();
        }

        if (result.IsSuccess)
        {
            TempData["Message"] = $"Appointment status set to {result.Value!.Status}.";
        }
        else
        {
            TempData["Error"] = string.Join(" ", result.Errors.Select(error => error.Message));
        }

        return RedirectToAction(nameof(Daily), new { date = date ?? Today() });
    }

    // Archive hides the appointment everywhere and keeps its status (PDR-09, DEC-022).
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Archive(int id, DateOnly? date, CancellationToken cancellationToken)
    {
        var result = await appointments.ArchiveAsync(id, cancellationToken);

        if (result.Errors.Any(error => error.Code == AppointmentService.NotFoundCode))
        {
            return NotFound();
        }

        if (result.IsSuccess)
        {
            TempData["Message"] = "Appointment archived.";
        }
        else
        {
            TempData["Error"] = string.Join(" ", result.Errors.Select(error => error.Message));
        }

        return RedirectToAction(nameof(Daily), new { date = date ?? Today() });
    }

    // Reschedule (FR-APT-004, DEC-027). Opens on the appointment's own day, or on the day in the Day box.
    [HttpGet]
    public async Task<IActionResult> Reschedule(int id, DateOnly? date, CancellationToken cancellationToken)
    {
        var result = await appointments.GetForRescheduleAsync(id, cancellationToken);
        if (result.Errors.Any(error => error.Code == AppointmentService.NotFoundCode))
        {
            return NotFound();
        }

        if (!result.IsSuccess)
        {
            TempData["Error"] = string.Join(" ", result.Errors.Select(error => error.Message));
            return RedirectToAction(nameof(Daily), new { date = Today() });
        }

        var appointment = result.Value!;
        var day = date ?? DateOnly.FromDateTime(appointment.StartTime);

        return View(await BuildRescheduleFormAsync(appointment, day, startTime: null, cancellationToken));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Reschedule(AppointmentFormViewModel model, CancellationToken cancellationToken)
    {
        var current = await appointments.GetForRescheduleAsync(model.AppointmentId ?? 0, cancellationToken);
        if (current.Errors.Any(error => error.Code == AppointmentService.NotFoundCode))
        {
            return NotFound();
        }

        if (!current.IsSuccess)
        {
            TempData["Error"] = string.Join(" ", current.Errors.Select(error => error.Message));
            return RedirectToAction(nameof(Daily), new { date = Today() });
        }

        var appointment = current.Value!;
        var day = model.Date ?? DateOnly.FromDateTime(appointment.StartTime);

        if (model.StartTime is null)
        {
            ModelState.AddModelError(nameof(model.StartTime), "Choose a slot.");
            return View(await BuildRescheduleFormAsync(appointment, day, startTime: null, cancellationToken));
        }

        // Same rule as booking: the chosen slot must be on the day shown in the Day box.
        if (DateOnly.FromDateTime(model.StartTime.Value) != day)
        {
            ModelState.AddModelError(nameof(model.Date),
                "The chosen slot is on a different day. Choose the day, select Show slots, then pick a slot.");
            return View(await BuildRescheduleFormAsync(appointment, day, model.StartTime, cancellationToken));
        }

        var result = await appointments.RescheduleAsync(appointment.Id, model.StartTime.Value, cancellationToken);
        if (!result.IsSuccess)
        {
            AddErrors(result);
            return View(await BuildRescheduleFormAsync(appointment, day, model.StartTime, cancellationToken));
        }

        TempData["Message"] =
            $"Appointment moved to {model.StartTime.Value:dd-MMM-yyyy} at {model.StartTime.Value:HH:mm}.";

        return RedirectToAction(nameof(Daily), new { date = DateOnly.FromDateTime(model.StartTime.Value) });
    }

    private async Task<AppointmentFormViewModel> BuildRescheduleFormAsync(
        AppointmentDto appointment, DateOnly day, DateTime? startTime, CancellationToken cancellationToken)
    {
        var slots = await appointments.GetSlotsAsync(day, cancellationToken);

        return new AppointmentFormViewModel
        {
            AppointmentId = appointment.Id,
            PatientId = appointment.PatientId,
            PatientName = appointment.PatientName,
            PatientCode = appointment.PatientCode,
            CurrentStartTime = appointment.StartTime,
            Date = day,
            StartTime = startTime,
            Slots = slots
                .Select(slot =>
                {
                    // The appointment's own slot is shown free, marked as the current time.
                    var isCurrent = slot.StartTime == appointment.StartTime;
                    var label = slot.StartTime.ToString("HH:mm", CultureInfo.InvariantCulture) + (isCurrent ? " (current)" : string.Empty);
                    return new SlotViewModel(slot.StartTime, label, slot.IsTaken && !isCurrent);
                })
                .ToList()
        };
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
