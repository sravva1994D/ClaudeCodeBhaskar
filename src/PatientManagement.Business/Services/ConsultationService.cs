using PatientManagement.Business.Appointments;
using PatientManagement.Business.Common;
using PatientManagement.Business.DTOs;
using PatientManagement.Business.Interfaces;
using PatientManagement.Business.Validators;
using PatientManagement.Data.Abstractions;
using PatientManagement.Data.Entities;

namespace PatientManagement.Business.Services;

public sealed class ConsultationService(
    IConsultationRepository consultations,
    IPrescriptionRepository prescriptions,
    IAppointmentRepository appointments,
    IPatientRepository patients,
    TimeProvider timeProvider) : IConsultationService
{
    public const string PatientNotFoundCode = "Consultation.Patient.NotFound";
    public const string NotFoundCode = VisitErrors.NotFoundCode;
    public const string NotFoundMessage = VisitErrors.NotFoundMessage;
    public const string AppointmentNotFoundCode = "Consultation.Appointment.NotFound";
    public const string AppointmentOtherPatientCode = "Consultation.Appointment.OtherPatient";
    public const string AppointmentNotCheckedInCode = "Consultation.Appointment.NotCheckedIn";
    public const string AppointmentAlreadyVisitedCode = "Consultation.Appointment.AlreadyVisited";
    public const string AlreadyFinishedCode = "Consultation.Finish.AlreadyFinished";
    public const string FinishStatusCode = "Consultation.Finish.Status";

    public async Task<Result<ConsultationDto>> StartAsync(
        int patientId, int? appointmentId, CancellationToken cancellationToken = default)
    {
        var patient = await patients.GetByIdAsync(patientId, cancellationToken);
        if (patient is null)
        {
            return Result<ConsultationDto>.Fail(new ResultMessage(PatientNotFoundCode, "The patient was not found."));
        }

        // Walk-in: no appointment, so no status changes (OQ-006, DEC-022).
        if (appointmentId is null)
        {
            var walkIn = new Consultation { PatientId = patient.Id, VisitDateTime = Now() };
            await consultations.AddAsync(walkIn, linkedAppointment: null, cancellationToken);

            return Result<ConsultationDto>.Ok(ConsultationMapper.ToDto(walkIn, patient));
        }

        var appointment = await appointments.GetByIdAsync(appointmentId.Value, cancellationToken);
        if (appointment is null || appointment.IsArchived)
        {
            return Result<ConsultationDto>.Fail(new ResultMessage(AppointmentNotFoundCode, "The appointment was not found."));
        }

        if (appointment.PatientId != patient.Id)
        {
            return Result<ConsultationDto>.Fail(new ResultMessage(AppointmentOtherPatientCode,
                "This appointment belongs to another patient."));
        }

        // Only a checked-in appointment starts a consultation (19 §4, §6). Name the current status in the message.
        if (appointment.Status != AppointmentStatus.CheckedIn)
        {
            return Result<ConsultationDto>.Fail(new ResultMessage(AppointmentNotCheckedInCode,
                $"This appointment is {appointment.Status}. Only a checked-in appointment can start a consultation."));
        }

        if (await consultations.GetByAppointmentIdAsync(appointment.Id, cancellationToken) is not null)
        {
            return AlreadyVisited();
        }

        appointment.Status = AppointmentStatus.InConsultation;
        var visit = new Consultation
        {
            PatientId = patient.Id,
            AppointmentId = appointment.Id,
            VisitDateTime = Now()
        };

        try
        {
            // The visit and the appointment's new status are saved together (Module 4 plan §7).
            await consultations.AddAsync(visit, appointment, cancellationToken);
        }
        catch (AppointmentAlreadyVisitedException)
        {
            // A second start for the same appointment raced past the check above (PDR-26).
            return AlreadyVisited();
        }

        return Result<ConsultationDto>.Ok(ConsultationMapper.ToDto(visit, patient));
    }

    public async Task<Result<ConsultationDto>> GetAsync(int consultationId, CancellationToken cancellationToken = default)
    {
        var visit = await consultations.GetByIdAsync(consultationId, includeVitals: true, cancellationToken);
        if (visit is null)
        {
            return NotFound();
        }

        var patient = await patients.GetByIdAsync(visit.PatientId, cancellationToken);
        if (patient is null)
        {
            return NotFound();
        }

        return Result<ConsultationDto>.Ok(ConsultationMapper.ToDto(visit, patient));
    }

    public async Task<Result<ConsultationDto>> SaveAsync(
        int consultationId, ConsultationSaveInput input, bool confirmWarnings, CancellationToken cancellationToken = default)
    {
        var visit = await consultations.GetByIdAsync(consultationId, includeVitals: true, cancellationToken);
        if (visit is null)
        {
            return NotFound();
        }

        var patient = await patients.GetByIdAsync(visit.PatientId, cancellationToken);
        if (patient is null)
        {
            return NotFound();
        }

        // Once the prescription is printed, the visit's text and vitals are read-only (FR-RX-002 AC-4, D-M5-4).
        if ((await prescriptions.GetByConsultationAsync(consultationId, cancellationToken))?.IsPrinted == true)
        {
            return PrintedVisit();
        }

        // Errors win over warnings (13 §4): nothing is saved while an error exists.
        var errors = VitalsValidator.Errors(input.Vitals);
        if (errors.Count > 0)
        {
            return Result<ConsultationDto>.Fail(errors.ToArray());
        }

        var warnings = VitalsValidator.Warnings(input.Vitals);
        if (warnings.Count > 0 && !confirmWarnings)
        {
            return Result<ConsultationDto>.ConfirmationRequired(warnings);
        }

        var notices = VitalsValidator.Notices(input.Vitals);

        visit.Complaints = Clean(input.Complaints);
        visit.Diagnosis = Clean(input.Diagnosis);

        // Empty vitals leave saved vitals as they are: the application never deletes clinical data (CLAUDE §6).
        if (!VitalsValidator.IsEmpty(input.Vitals))
        {
            SetVitals(visit, input.Vitals);
        }

        // The text and the vitals are saved together, and only while the prescription is unprinted (D-M5-8, FR-RX-002 AC-4).
        // The repository repeats the printed check under a lock, so a print that finishes first cannot be overtaken.
        if (!await consultations.UpdateAsync(visit, cancellationToken))
        {
            return PrintedVisit();
        }

        var saved = await consultations.GetByIdAsync(visit.Id, includeVitals: true, cancellationToken);

        return Result<ConsultationDto>.Ok(ConsultationMapper.ToDto(saved!, patient), warnings.Concat(notices));
    }

    public async Task<Result<ConsultationDto>> FinishAsync(int consultationId, CancellationToken cancellationToken = default)
    {
        var visit = await consultations.GetByIdAsync(consultationId, includeVitals: true, cancellationToken);
        if (visit is null)
        {
            return NotFound();
        }

        var patient = await patients.GetByIdAsync(visit.PatientId, cancellationToken);
        if (patient is null)
        {
            return NotFound();
        }

        // Finish is one-time (D-M4-3). The save below enforces it too, for requests that overlap.
        if (visit.FinishedAt is not null)
        {
            return AlreadyFinished();
        }

        Appointment? appointment = null;
        if (visit.AppointmentId is { } appointmentId)
        {
            appointment = await appointments.GetByIdAsync(appointmentId, cancellationToken);
            if (appointment is null)
            {
                return Result<ConsultationDto>.Fail(new ResultMessage(AppointmentNotFoundCode, "The appointment was not found."));
            }

            // Finish is the only action that sets Completed, and only from In Consultation (PDR-27, DEC-022).
            if (!AppointmentLifecycle.CanMove(appointment.Status, AppointmentStatus.Completed))
            {
                return Result<ConsultationDto>.Fail(new ResultMessage(FinishStatusCode,
                    $"The appointment is {appointment.Status}. Finish needs the appointment to be In Consultation."));
            }

            appointment.Status = AppointmentStatus.Completed;
        }

        var finishedAt = Now();
        if (!await consultations.MarkFinishedAsync(visit.Id, finishedAt, appointment, cancellationToken))
        {
            return AlreadyFinished();
        }

        visit.FinishedAt = finishedAt;

        return Result<ConsultationDto>.Ok(ConsultationMapper.ToDto(visit, patient));
    }

    public async Task<IReadOnlyList<VisitSummaryDto>> ListVisitsAsync(int patientId, CancellationToken cancellationToken = default)
    {
        var visits = await consultations.ListByPatientAsync(patientId, cancellationToken);

        return visits.Select(ConsultationMapper.ToSummary).ToList();
    }

    private DateTime Now() => timeProvider.GetLocalNow().DateTime;

    // Blank text is stored as no text. Other text is kept as entered, apart from surrounding spaces.
    private static string? Clean(string? text) =>
        string.IsNullOrWhiteSpace(text) ? null : text.Trim();

    private static Result<ConsultationDto> NotFound() =>
        Result<ConsultationDto>.Fail(new ResultMessage(NotFoundCode, NotFoundMessage));

    // Changes the visit's vitals in place, or adds them when none were saved yet (1:0..1). Only called when the vitals are not empty.
    private static void SetVitals(Consultation visit, VitalsInput input)
    {
        var vitals = visit.Vitals ?? (visit.Vitals = new VitalSign { ConsultationId = visit.Id });
        vitals.TemperatureC = input.TemperatureC!.Value;
        vitals.SystolicMmHg = (short)input.SystolicMmHg!.Value;
        vitals.DiastolicMmHg = (short)input.DiastolicMmHg!.Value;
        vitals.PulseBpm = (short)input.PulseBpm!.Value;
    }

    private static Result<ConsultationDto> PrintedVisit() =>
        Result<ConsultationDto>.Fail(new ResultMessage(VisitErrors.PrintedCode, VisitErrors.PrintedVisitMessage));

    private static Result<ConsultationDto> AlreadyFinished() =>
        Result<ConsultationDto>.Fail(new ResultMessage(AlreadyFinishedCode, "This visit is already finished."));

    private static Result<ConsultationDto> AlreadyVisited() =>
        Result<ConsultationDto>.Fail(new ResultMessage(AppointmentAlreadyVisitedCode,
            "A consultation already exists for this appointment."));
}
