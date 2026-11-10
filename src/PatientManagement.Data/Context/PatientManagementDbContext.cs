using Microsoft.EntityFrameworkCore;

namespace PatientManagement.Data.Context;

/// <summary>
/// The single EF Core context for the application (AD-04; PDR-32: EF Core only).
/// No DbSet is declared yet. Entities are added with the feature work that needs them.
/// </summary>
public class PatientManagementDbContext(DbContextOptions<PatientManagementDbContext> options) : DbContext(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
    }
}
