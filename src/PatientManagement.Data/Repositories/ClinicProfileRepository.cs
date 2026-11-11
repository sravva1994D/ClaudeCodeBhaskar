using Microsoft.EntityFrameworkCore;
using PatientManagement.Data.Abstractions;
using PatientManagement.Data.Context;
using PatientManagement.Data.Entities;

namespace PatientManagement.Data.Repositories;

public sealed class ClinicProfileRepository(PatientManagementDbContext db) : IClinicProfileRepository
{
    // The table holds one row, always Id = 1 (PDR-15; database CHECK).
    private const int SingleRowId = 1;

    public Task<ClinicProfile?> GetAsync(CancellationToken cancellationToken = default) =>
        db.ClinicProfiles.AsNoTracking().SingleOrDefaultAsync(profile => profile.Id == SingleRowId, cancellationToken);

    public async Task<ClinicProfile> GetOrCreateAsync(CancellationToken cancellationToken = default)
    {
        var existing = await GetAsync(cancellationToken);
        if (existing is not null)
        {
            return existing;
        }

        var profile = new ClinicProfile { Id = SingleRowId };
        try
        {
            db.ClinicProfiles.Add(profile);
            await db.SaveChangesAsync(cancellationToken);

            return profile;
        }
        catch (DbUpdateException exception) when (UniqueViolation.IsUniqueViolation(exception))
        {
            // Another request created the row between the read and the insert. Use that row.
            db.Entry(profile).State = EntityState.Detached;

            return (await GetAsync(cancellationToken))!;
        }
    }

    public async Task UpsertAsync(ClinicProfile profile, CancellationToken cancellationToken = default)
    {
        var existing = await db.ClinicProfiles.FindAsync([SingleRowId], cancellationToken);

        if (existing is null)
        {
            profile.Id = SingleRowId;
            db.ClinicProfiles.Add(profile);
        }
        else
        {
            existing.ClinicName = profile.ClinicName;
            existing.Address = profile.Address;
            existing.Phone = profile.Phone;
            existing.DoctorName = profile.DoctorName;
            existing.RegistrationNumber = profile.RegistrationNumber;
        }

        await db.SaveChangesAsync(cancellationToken);
    }
}
