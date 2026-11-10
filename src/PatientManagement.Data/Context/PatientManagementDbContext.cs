using Microsoft.EntityFrameworkCore;
using PatientManagement.Data.Configurations;
using PatientManagement.Data.Entities;

namespace PatientManagement.Data.Context;

/// <summary>
/// The single EF Core context for the application (AD-04; PDR-32: EF Core only).
/// Each entity is added with the feature work that needs it.
/// </summary>
public class PatientManagementDbContext(DbContextOptions<PatientManagementDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfiguration(new UserConfiguration());
    }
}
