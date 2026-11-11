using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PatientManagement.Data.Entities;

namespace PatientManagement.Data.Configurations;

// Column rules and indexes from Documentation/02-Planning/07-Database-Design.md §2 (Prescriptions), §3 and §4.
public class PrescriptionConfiguration : IEntityTypeConfiguration<Prescription>
{
    public void Configure(EntityTypeBuilder<Prescription> builder)
    {
        builder.ToTable("Prescriptions");

        builder.HasKey(prescription => prescription.Id);

        // Prescriptions are kept with the visit (restrict).
        builder.HasOne<Consultation>()
            .WithMany()
            .HasForeignKey(prescription => prescription.ConsultationId)
            .OnDelete(DeleteBehavior.Restrict);

        // One prescription per consultation (FR-RX-002 AC-3, PDR-14).
        builder.HasIndex(prescription => prescription.ConsultationId)
            .IsUnique()
            .HasDatabaseName("IX_Prescriptions_ConsultationId");

        builder.Property(prescription => prescription.IsPrinted).HasDefaultValue(false).IsRequired();

        builder.Property(prescription => prescription.PrintedAt).HasColumnType("datetime2(0)");

        // Medicine lines belong to the prescription and are removed with it (cascade).
        builder.HasMany(prescription => prescription.Items)
            .WithOne(item => item.Prescription)
            .HasForeignKey(item => item.PrescriptionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
