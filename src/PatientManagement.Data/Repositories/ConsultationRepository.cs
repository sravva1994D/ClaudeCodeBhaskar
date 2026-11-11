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

    public async Task<IReadOnlyList<Consultation>> ListByPatientAsync(int patientId, CancellationToken cancellationToken = default) =>
        await db.Consultations
            .AsNoTracking()
            .Where(consultation => consultation.PatientId == patientId)
            .OrderByDescending(consultation => consultation.VisitDateTime)
            .ThenByDescending(consultation => consultation.Id)
            .ToListAsync(cancellationToken);

    public Task<DateTime?> GetLatestVisitTimeAsync(int patientId, CancellationToken cancellationToken = default) =>
        db.Consultations
            .Where(consultation => consultation.PatientId == patientId)
            .Select(consultation => (DateTime?)consultation.VisitDateTime)
            .MaxAsync(cancellationToken);

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

    public async Task UpdateAsync(Consultation consultation, Appointment? linkedAppointment = null, CancellationToken cancellationToken = default)
    {
        db.Consultations.Update(consultation);
        MarkLinkedAppointment(linkedAppointment);
        await db.SaveChangesAsync(cancellationToken);
    }

    private void MarkLinkedAppointment(Appointment? linkedAppointment)
    {
        if (linkedAppointment is not null)
        {
            db.Appointments.Update(linkedAppointment);
        }
    }
}
