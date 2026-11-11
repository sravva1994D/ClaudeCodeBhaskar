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

    public DbSet<Patient> Patients => Set<Patient>();

    public DbSet<Appointment> Appointments => Set<Appointment>();

    public DbSet<Consultation> Consultations => Set<Consultation>();

    public DbSet<VitalSign> VitalSigns => Set<VitalSign>();

    public DbSet<Prescription> Prescriptions => Set<Prescription>();

    public DbSet<PrescriptionItem> PrescriptionItems => Set<PrescriptionItem>();

    public DbSet<ClinicProfile> ClinicProfiles => Set<ClinicProfile>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfiguration(new UserConfiguration());
        modelBuilder.ApplyConfiguration(new PatientConfiguration());
        modelBuilder.ApplyConfiguration(new AppointmentConfiguration());
        modelBuilder.ApplyConfiguration(new ConsultationConfiguration());
        modelBuilder.ApplyConfiguration(new VitalSignConfiguration());
        modelBuilder.ApplyConfiguration(new PrescriptionConfiguration());
        modelBuilder.ApplyConfiguration(new PrescriptionItemConfiguration());
        modelBuilder.ApplyConfiguration(new ClinicProfileConfiguration());
    }
}
