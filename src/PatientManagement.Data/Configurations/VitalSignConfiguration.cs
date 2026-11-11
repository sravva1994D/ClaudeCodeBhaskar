using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PatientManagement.Data.Entities;

namespace PatientManagement.Data.Configurations;

// Column rules from Documentation/02-Planning/07-Database-Design.md §2 (VitalSigns). No range checks: AD-14.
public class VitalSignConfiguration : IEntityTypeConfiguration<VitalSign>
{
    public void Configure(EntityTypeBuilder<VitalSign> builder)
    {
        builder.ToTable("VitalSigns");

        builder.HasKey(vitals => vitals.ConsultationId);

        builder.Property(vitals => vitals.TemperatureC).HasColumnType("decimal(4,1)").IsRequired();
        builder.Property(vitals => vitals.SystolicMmHg).IsRequired();
        builder.Property(vitals => vitals.DiastolicMmHg).IsRequired();
        builder.Property(vitals => vitals.PulseBpm).IsRequired();
    }
}
