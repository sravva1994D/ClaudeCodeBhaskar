using Microsoft.EntityFrameworkCore;
using PatientManagement.Data.Abstractions;
using PatientManagement.Data.Context;
using PatientManagement.Data.Entities;

namespace PatientManagement.Data.Repositories;

public sealed class ConsultationRepository(PatientManagementDbContext db) : IConsultationRepository
{
    public Task<Consultation?> GetByIdAsync(int id, bool includeVitals = false, CancellationToken cancellationToken = default)
    {
        var query = db.Consultations.AsQueryable();
        if (includeVitals)
        {
            query = query.Include(consultation => consultation.Vitals);
        }

        return query.SingleOrDefaultAsync(consultation => consultation.Id == id, cancellationToken);
    }

    public Task<Consultation?> GetByAppointmentIdAsync(int appointmentId, CancellationToken cancellationToken = default) =>
        db.Consultations.SingleOrDefaultAsync(consultation => consultation.AppointmentId == appointmentId, cancellationToken);

    public async Task<IReadOnlyList<Consultation>> ListByPatientAsync(
        int patientId, DateOnly? from = null, DateOnly? to = null, CancellationToken cancellationToken = default)
    {
        // The upper bound is the start of the day after to, so the whole to day is included (FR-HIS-003 AC-2).
        // The last day the type can hold has no day after it, so it needs no upper bound (and AddDays would overflow).
        var start = from?.ToDateTime(TimeOnly.MinValue);
        var endExclusive = to is { } last && last < DateOnly.MaxValue
            ? last.AddDays(1).ToDateTime(TimeOnly.MinValue)
            : (DateTime?)null;

        return await db.Consultations
            .AsNoTracking()
            .Where(consultation => consultation.PatientId == patientId)
            .Where(consultation => start == null || consultation.VisitDateTime >= start)
            .Where(consultation => endExclusive == null || consultation.VisitDateTime < endExclusive)
            .OrderByDescending(consultation => consultation.VisitDateTime)
            .ThenByDescending(consultation => consultation.Id)
            .ToListAsync(cancellationToken);
    }

    public Task<DateTime?> GetLatestVisitTimeAsync(int patientId, CancellationToken cancellationToken = default) =>
        db.Consultations
            .Where(consultation => consultation.PatientId == patientId)
            .Select(consultation => (DateTime?)consultation.VisitDateTime)
            .MaxAsync(cancellationToken);

    public async Task<IReadOnlyList<PatientLastVisit>> GetRecentPatientVisitsAsync(int count, CancellationToken cancellationToken = default) =>
        (await db.Consultations
            .GroupBy(consultation => consultation.PatientId)
            .Select(group => new { PatientId = group.Key, LastVisit = group.Max(consultation => consultation.VisitDateTime) })
            .OrderByDescending(row => row.LastVisit)
            .ThenBy(row => row.PatientId)
            .Take(count)
            .AsNoTracking()
            .ToListAsync(cancellationToken))
            .Select(row => new PatientLastVisit(row.PatientId, row.LastVisit))
            .ToList();

    public async Task AddAsync(Consultation consultation, Appointment? linkedAppointment = null, CancellationToken cancellationToken = default)
    {
        db.Consultations.Add(consultation);
        MarkLinkedAppointment(linkedAppointment);

        try
        {
            // The visit and its appointment change are saved together, or not at all.
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception) when (UniqueViolation.IsUniqueViolation(exception))
        {
            throw new AppointmentAlreadyVisitedException(exception);
        }
    }

    public async Task<bool> UpdateAsync(Consultation consultation, CancellationToken cancellationToken = default)
    {
        await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);

        // The no-op update takes the prescription's row lock and matches only while it is unprinted. A concurrent Print
        // waits for this save; if the print comes first, the check below refuses the save (AD-13, FR-RX-002 AC-4).
        var unlocked = await db.Prescriptions
            .Where(prescription => prescription.ConsultationId == consultation.Id && !prescription.IsPrinted)
            .ExecuteUpdateAsync(setters => setters.SetProperty(prescription => prescription.IsPrinted, prescription => prescription.IsPrinted),
                cancellationToken);
        if (unlocked == 0 && await db.Prescriptions.AnyAsync(prescription => prescription.ConsultationId == consultation.Id, cancellationToken))
        {
            return false;
        }

        if (db.Entry(consultation).State == EntityState.Detached)
        {
            db.Consultations.Update(consultation);
        }

        // Vitals that were not saved before are new rows; saved vitals are updated by change tracking.
        if (consultation.Vitals is { } vitals && db.Entry(vitals).State == EntityState.Detached)
        {
            db.Entry(vitals).State = EntityState.Added;
        }

        await db.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return true;
    }

    public async Task<bool> MarkFinishedAsync(
        int consultationId, DateTime finishedAt, Appointment? linkedAppointment = null, CancellationToken cancellationToken = default)
    {
        // The conditional UPDATE takes the row lock. A second overlapping Finish waits, then matches no row.
        // Bulk updates skip the change tracker, so nothing else is written for these entities.
        await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);

        var marked = await db.Consultations
            .Where(consultation => consultation.Id == consultationId && consultation.FinishedAt == null)
            .ExecuteUpdateAsync(setters => setters.SetProperty(consultation => consultation.FinishedAt, (DateTime?)finishedAt),
                cancellationToken);
        if (marked == 0)
        {
            return false;
        }

        if (linkedAppointment is not null)
        {
            await db.Appointments
                .Where(appointment => appointment.Id == linkedAppointment.Id)
                .ExecuteUpdateAsync(setters => setters.SetProperty(appointment => appointment.Status, linkedAppointment.Status),
                    cancellationToken);
        }

        await transaction.CommitAsync(cancellationToken);

        return true;
    }

    private void MarkLinkedAppointment(Appointment? linkedAppointment)
    {
        if (linkedAppointment is not null)
        {
            db.Appointments.Update(linkedAppointment);
        }
    }
}
