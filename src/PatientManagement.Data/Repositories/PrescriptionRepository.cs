using Microsoft.EntityFrameworkCore;
using PatientManagement.Data.Abstractions;
using PatientManagement.Data.Context;
using PatientManagement.Data.Entities;

namespace PatientManagement.Data.Repositories;

public sealed class PrescriptionRepository(PatientManagementDbContext db) : IPrescriptionRepository
{
    public Task<Prescription?> GetByConsultationAsync(int consultationId, CancellationToken cancellationToken = default) =>
        db.Prescriptions
            .AsNoTracking()
            .Include(prescription => prescription.Items.OrderBy(item => item.SortOrder).ThenBy(item => item.Id))
            .SingleOrDefaultAsync(prescription => prescription.ConsultationId == consultationId, cancellationToken);

    public async Task AddAsync(Prescription prescription, CancellationToken cancellationToken = default)
    {
        db.Prescriptions.Add(prescription);

        try
        {
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception) when (UniqueViolation.IsUniqueViolation(exception))
        {
            throw new PrescriptionAlreadyExistsException(exception);
        }
    }

    public async Task<bool> ReplaceItemsAsync(int prescriptionId, IReadOnlyList<PrescriptionItem> items,
        CancellationToken cancellationToken = default)
    {
        await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);

        // The no-op update takes the prescription's row lock and matches only while it is unprinted.
        // A concurrent Print waits for this transaction, then finds the lines already replaced.
        var unlocked = await db.Prescriptions
            .Where(prescription => prescription.Id == prescriptionId && !prescription.IsPrinted)
            .ExecuteUpdateAsync(setters => setters.SetProperty(prescription => prescription.IsPrinted, prescription => prescription.IsPrinted),
                cancellationToken);
        if (unlocked == 0)
        {
            return false;
        }

        await db.PrescriptionItems
            .Where(item => item.PrescriptionId == prescriptionId)
            .ExecuteDeleteAsync(cancellationToken);

        foreach (var item in items)
        {
            item.PrescriptionId = prescriptionId;
        }

        db.PrescriptionItems.AddRange(items);
        await db.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return true;
    }

    public async Task<bool> MarkPrintedAsync(int prescriptionId, DateTime printedAt, CancellationToken cancellationToken = default)
    {
        // One conditional UPDATE: it sets the flag only once, and only when at least one line exists (PDR-13).
        var marked = await db.Prescriptions
            .Where(prescription => prescription.Id == prescriptionId && !prescription.IsPrinted && prescription.Items.Any())
            .ExecuteUpdateAsync(setters => setters
                    .SetProperty(prescription => prescription.IsPrinted, true)
                    .SetProperty(prescription => prescription.PrintedAt, (DateTime?)printedAt),
                cancellationToken);

        return marked > 0;
    }
}
