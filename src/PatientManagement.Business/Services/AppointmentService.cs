using PatientManagement.Business.Appointments;
using PatientManagement.Business.Common;
using PatientManagement.Business.DTOs;
using PatientManagement.Business.Interfaces;
using PatientManagement.Business.Validators;
using PatientManagement.Data.Abstractions;
using PatientManagement.Data.Entities;

namespace PatientManagement.Business.Services;

public sealed class AppointmentService(
    IAppointmentRepository appointments,
    IPatientRepository patients,
    TimeProvider timeProvider) : IAppointmentService
{
    public const string NotFoundCode = "Appointment.NotFound";
    public const string NotFoundMessage = "The appointment was not found.";
    public const string PatientNotFoundCode = "Appointment.Patient.NotFound";
    public const string PatientNotFoundMessage = "Choose a registered patient.";
    public const string SlotTakenCode = "Appointment.Slot.Taken";
    public const string SlotTakenMessage = "That slot is already booked. Choose another slot.";
    public const string NotScheduledCode = "Appointment.Reschedule.NotScheduled";
    public const string NotScheduledMessage = "Only a scheduled appointment can be rescheduled.";
    public const string SameSlotCode = "Appointment.Reschedule.SameSlot";
    public const string SameSlotMessage = "Choose a different slot. The appointment is already at this time.";

    public async Task<Result<AppointmentDto>> ScheduleAsync(
        AppointmentCreateDto input, CancellationToken cancellationToken = default)
    {
        // Cheap rules first: they need no database read.
        var errors = AppointmentValidator.ValidateSchedule(input.StartTime, Today());
        if (errors.Count > 0)
        {
            return Result<AppointmentDto>.Fail(errors.ToArray());
        }

        var patient = await patients.GetByIdAsync(input.PatientId, cancellationToken);
        if (patient is null)
        {
            return Result<AppointmentDto>.Fail(new ResultMessage(PatientNotFoundCode, PatientNotFoundMessage));
        }

        if (await appointments.HasOverlapAsync(input.StartTime, excludeId: null, cancellationToken))
        {
            return SlotTaken();
        }

        var appointment = new Appointment
        {
            PatientId = patient.Id,
            StartTime = input.StartTime,
            DurationMinutes = (short)SlotGrid.DurationMinutes,
            Status = AppointmentStatus.Scheduled,
            IsArchived = false
        };

        try
        {
            await appointments.AddAsync(appointment, cancellationToken);
        }
        catch (SlotTakenException)
        {
            // Another booking took the slot after the check above (PDR-07). The database index refused the save.
            return SlotTaken();
        }

        return Result<AppointmentDto>.Ok(AppointmentMapper.ToDto(appointment, patient));
    }

    public async Task<IReadOnlyList<AppointmentDto>> GetDailyAsync(
        DateOnly date, CancellationToken cancellationToken = default)
    {
        var daily = await appointments.GetDailyAsync(date, cancellationToken);

        return daily.Select(appointment => AppointmentMapper.ToDto(appointment, appointment.Patient)).ToList();
    }

    public async Task<IReadOnlyList<SlotDto>> GetSlotsAsync(DateOnly date, CancellationToken cancellationToken = default)
    {
        var daily = await appointments.GetDailyAsync(date, cancellationToken);

        // Only statuses that hold the slot count as taken. A cancelled or no-show booking leaves the slot free (PDR-07).
        var taken = daily
            .Where(appointment => AppointmentStatus.HoldsSlot(appointment.Status))
            .Select(appointment => appointment.StartTime)
            .ToHashSet();

        return SlotGrid.SlotsFor(date)
            .Select(slot => new SlotDto(slot, taken.Contains(slot)))
            .ToList();
    }

    public async Task<Result<AppointmentDto>> UpdateStatusAsync(
        int id, string status, CancellationToken cancellationToken = default)
    {
        var appointment = await appointments.GetByIdAsync(id, cancellationToken);
        if (appointment is null || appointment.IsArchived)
        {
            return NotFound();
        }

        var errors = AppointmentValidator.ValidateStatusChange(appointment.Status, status);
        if (errors.Count > 0)
        {
            return Result<AppointmentDto>.Fail(errors.ToArray());
        }

        appointment.Status = status;
        await appointments.UpdateAsync(appointment, cancellationToken);

        return Result<AppointmentDto>.Ok(AppointmentMapper.ToDto(appointment, appointment.Patient));
    }

    public async Task<Result<AppointmentDto>> ArchiveAsync(int id, CancellationToken cancellationToken = default)
    {
        var appointment = await appointments.GetByIdAsync(id, cancellationToken);
        if (appointment is null || appointment.IsArchived)
        {
            return NotFound();
        }

        var errors = AppointmentValidator.ValidateArchive(appointment.Status);
        if (errors.Count > 0)
        {
            return Result<AppointmentDto>.Fail(errors.ToArray());
        }

        // Archiving hides the appointment and keeps its status (PDR-09, DEC-022).
        appointment.IsArchived = true;
        await appointments.UpdateAsync(appointment, cancellationToken);

        return Result<AppointmentDto>.Ok(AppointmentMapper.ToDto(appointment, appointment.Patient));
    }

    public async Task<Result<AppointmentDto>> GetForRescheduleAsync(int id, CancellationToken cancellationToken = default)
    {
        var appointment = await appointments.GetByIdAsync(id, cancellationToken);
        if (appointment is null || appointment.IsArchived)
        {
            return NotFound();
        }

        if (appointment.Status != AppointmentStatus.Scheduled)
        {
            return NotScheduled();
        }

        return Result<AppointmentDto>.Ok(AppointmentMapper.ToDto(appointment, appointment.Patient));
    }

    public async Task<Result<AppointmentDto>> RescheduleAsync(
        int id, DateTime newStartTime, CancellationToken cancellationToken = default)
    {
        var appointment = await appointments.GetByIdAsync(id, cancellationToken);
        if (appointment is null || appointment.IsArchived)
        {
            return NotFound();
        }

        // Only a Scheduled appointment moves. Once checked in, the appointment is fixed (FR-APT-004).
        if (appointment.Status != AppointmentStatus.Scheduled)
        {
            return NotScheduled();
        }

        var errors = AppointmentValidator.ValidateSchedule(newStartTime, Today());
        if (errors.Count > 0)
        {
            return Result<AppointmentDto>.Fail(errors.ToArray());
        }

        if (newStartTime == appointment.StartTime)
        {
            return Result<AppointmentDto>.Fail(new ResultMessage(SameSlotCode, SameSlotMessage, "StartTime"));
        }

        // The appointment's own current slot does not count as taken (excludeId).
        if (await appointments.HasOverlapAsync(newStartTime, excludeId: appointment.Id, cancellationToken))
        {
            return SlotTaken();
        }

        appointment.StartTime = newStartTime;

        try
        {
            await appointments.UpdateAsync(appointment, cancellationToken);
        }
        catch (SlotTakenException)
        {
            // Another booking took the slot after the check above. The database index refused the move (PDR-07).
            return SlotTaken();
        }

        return Result<AppointmentDto>.Ok(AppointmentMapper.ToDto(appointment, appointment.Patient));
    }

    private static Result<AppointmentDto> NotScheduled() =>
        Result<AppointmentDto>.Fail(new ResultMessage(NotScheduledCode, NotScheduledMessage));

    private DateOnly Today() => DateOnly.FromDateTime(timeProvider.GetLocalNow().DateTime);

    private static Result<AppointmentDto> SlotTaken() =>
        Result<AppointmentDto>.Fail(new ResultMessage(SlotTakenCode, SlotTakenMessage, "StartTime"));

    private static Result<AppointmentDto> NotFound() =>
        Result<AppointmentDto>.Fail(new ResultMessage(NotFoundCode, NotFoundMessage));
}
