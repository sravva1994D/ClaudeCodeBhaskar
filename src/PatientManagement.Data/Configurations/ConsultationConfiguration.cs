using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PatientManagement.Data.Entities;

namespace PatientManagement.Data.Configurations;

// Column rules and indexes from Documentation/02-Planning/07-Database-Design.md §2 (Consultations, VitalSigns) and §4.
public class ConsultationConfiguration : IEntityTypeConfiguration<Consultation>
{
    public void Configure(EntityTypeBuilder<Consultation> builder)
    {
        builder.ToTable("Consultations");

        builder.HasKey(consultation => consultation.Id);

        builder.HasOne<Patient>()
            .WithMany()
            .HasForeignKey(consultation => consultation.PatientId)
            .OnDelete(DeleteBehavior.Restrict);

        // The visit is kept if its appointment is removed (07 §3). The app never deletes appointments, so this is a safety net.
        builder.HasOne<Appointment>()
            .WithMany()
            .HasForeignKey(consultation => consultation.AppointmentId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Property(consultation => consultation.VisitDateTime).HasColumnType("datetime2(0)").IsRequired();
        builder.HasIndex(consultation => new { consultation.PatientId, consultation.VisitDateTime })
            .IsDescending(false, true)
            .HasDatabaseName("IX_Consultations_Patient_Visit");

        builder.Property(consultation => consultation.Complaints).HasColumnType("nvarchar(max)");
        builder.Property(consultation => consultation.Diagnosis).HasColumnType("nvarchar(max)");

        builder.Property(consultation => consultation.FinishedAt).HasColumnType("datetime2(0)");

        // One consultation per appointment (PDR-26). Walk-ins (null AppointmentId) are not limited.
        builder.HasIndex(consultation => consultation.AppointmentId)
            .IsUnique()
            .HasDatabaseName("IX_Consultations_AppointmentId")
            .HasFilter("[AppointmentId] IS NOT NULL");

        builder.HasOne(consultation => consultation.Vitals)
            .WithOne(vitals => vitals.Consultation)
            .HasForeignKey<VitalSign>(vitals => vitals.ConsultationId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
