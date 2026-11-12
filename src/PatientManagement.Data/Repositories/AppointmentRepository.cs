using Microsoft.EntityFrameworkCore;
using PatientManagement.Data.Abstractions;
using PatientManagement.Data.Context;
using PatientManagement.Data.Entities;

namespace PatientManagement.Data.Repositories;

public sealed class AppointmentRepository(PatientManagementDbContext db) : IAppointmentRepository
{
    // Statuses that release the slot (PDR-07). Kept in step with the slot index filter in AppointmentConfiguration.
    private const string Cancelled = "Cancelled";
    private const string NoShow = "No-show";

    public Task<Appointment?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        db.Appointments
            .Include(appointment => appointment.Patient)
            .SingleOrDefaultAsync(appointment => appointment.Id == id, cancellationToken);

    public async Task<IReadOnlyList<Appointment>> GetDailyAsync(DateOnly date, CancellationToken cancellationToken = default)
    {
        // Half-open range on local time: from midnight up to the next midnight. Computed here, not in the query.
        var dayStart = date.ToDateTime(TimeOnly.MinValue);
        var dayEnd = dayStart.AddDays(1);

        return await db.Appointments
            .AsNoTracking()
            .Include(appointment => appointment.Patient)
            .Where(appointment =>
                !appointment.IsArchived &&
                appointment.StartTime >= dayStart &&
                appointment.StartTime < dayEnd)
            .OrderBy(appointment => appointment.StartTime)
            .ThenBy(appointment => appointment.Id)
            .ToListAsync(cancellationToken);
    }

    public Task<bool> HasOverlapAsync(DateTime startTime, int? excludeId = null, CancellationToken cancellationToken = default) =>
        db.Appointments.AnyAsync(appointment =>
            appointment.StartTime == startTime &&
            !appointment.IsArchived &&
            appointment.Status != Cancelled &&
            appointment.Status != NoShow &&
            (excludeId == null || appointment.Id != excludeId),
            cancellationToken);

    public async Task AddAsync(Appointment appointment, CancellationToken cancellationToken = default)
    {
        db.Appointments.Add(appointment);

        try
        {
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception) when (UniqueViolation.IsUniqueViolation(exception))
        {
            throw new SlotTakenException(exception);
        }
    }

    public async Task UpdateAsync(Appointment appointment, CancellationToken cancellationToken = default)
    {
        db.Appointments.Update(appointment);

        try
        {
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception) when (UniqueViolation.IsUniqueViolation(exception))
        {
            // A reschedule can move an appointment onto a slot taken after the overlap check (PDR-07).
            throw new SlotTakenException(exception);
        }
    }
}
