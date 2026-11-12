using Microsoft.EntityFrameworkCore;
using PatientManagement.Data.Abstractions;
using PatientManagement.Data.Context;
using PatientManagement.Data.Entities;

namespace PatientManagement.Data.Repositories;

public sealed class PatientRepository(PatientManagementDbContext db) : IPatientRepository
{
    private const char LikeEscape = '\\';

    public Task<Patient?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        db.Patients.SingleOrDefaultAsync(patient => patient.Id == id, cancellationToken);

    public async Task<IReadOnlyList<Patient>> GetByIdsAsync(IReadOnlyCollection<int> ids, CancellationToken cancellationToken = default) =>
        await db.Patients
            .AsNoTracking()
            .Where(patient => ids.Contains(patient.Id))
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<Patient>> SearchAsync(string term, CancellationToken cancellationToken = default)
    {
        var pattern = "%" + EscapeLike(term) + "%";

        return await db.Patients
            .AsNoTracking()
            .Where(patient =>
                EF.Functions.Like(patient.FullName, pattern, LikeEscape.ToString()) ||
                EF.Functions.Like(patient.Phone, pattern, LikeEscape.ToString()))
            .OrderBy(patient => patient.FullName)
            .ThenBy(patient => patient.Id)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Patient>> FindPossibleDuplicatesAsync(
        string fullName, DateOnly dateOfBirth, string phone, int? excludeId = null,
        CancellationToken cancellationToken = default)
    {
        var lowerName = fullName.ToLower();

        return await db.Patients
            .AsNoTracking()
            .Where(patient =>
                (excludeId == null || patient.Id != excludeId) &&
                (patient.FullName.ToLower() == lowerName ||
                 patient.DateOfBirth == dateOfBirth ||
                 patient.Phone == phone))
            .OrderBy(patient => patient.FullName)
            .ThenBy(patient => patient.Id)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Patient patient, CancellationToken cancellationToken = default)
    {
        db.Patients.Add(patient);
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Patient patient, CancellationToken cancellationToken = default)
    {
        db.Patients.Update(patient);
        await db.SaveChangesAsync(cancellationToken);
    }

    // Makes %, _ and the escape character themselves, so a search term is never a wildcard pattern.
    private static string EscapeLike(string term) =>
        term.Replace("\\", "\\\\", StringComparison.Ordinal)
            .Replace("%", "\\%", StringComparison.Ordinal)
            .Replace("_", "\\_", StringComparison.Ordinal);
}
