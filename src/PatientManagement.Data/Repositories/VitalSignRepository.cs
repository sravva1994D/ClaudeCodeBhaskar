using Microsoft.EntityFrameworkCore;
using PatientManagement.Data.Abstractions;
using PatientManagement.Data.Context;
using PatientManagement.Data.Entities;

namespace PatientManagement.Data.Repositories;

public sealed class VitalSignRepository(PatientManagementDbContext db) : IVitalSignRepository
{
    public async Task UpsertAsync(VitalSign vitals, CancellationToken cancellationToken = default)
    {
        var existing = await db.VitalSigns.FindAsync([vitals.ConsultationId], cancellationToken);

        if (existing is null)
        {
            db.VitalSigns.Add(vitals);
        }
        else
        {
            existing.TemperatureC = vitals.TemperatureC;
            existing.SystolicMmHg = vitals.SystolicMmHg;
            existing.DiastolicMmHg = vitals.DiastolicMmHg;
            existing.PulseBpm = vitals.PulseBpm;
        }

        await db.SaveChangesAsync(cancellationToken);
    }
}
