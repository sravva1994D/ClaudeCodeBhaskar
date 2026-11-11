using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PatientManagement.Data.Entities;

namespace PatientManagement.Data.Configurations;

// Column rules from Documentation/02-Planning/07-Database-Design.md §2 (PrescriptionItems). Only Name is required (PDR-13).
public class PrescriptionItemConfiguration : IEntityTypeConfiguration<PrescriptionItem>
{
    public void Configure(EntityTypeBuilder<PrescriptionItem> builder)
    {
        builder.ToTable("PrescriptionItems");

        builder.HasKey(item => item.Id);

        builder.Property(item => item.Name).HasMaxLength(200).IsRequired();
        builder.Property(item => item.Dosage).HasMaxLength(100);
        builder.Property(item => item.Frequency).HasMaxLength(100);
        builder.Property(item => item.Duration).HasMaxLength(100);
        builder.Property(item => item.Instructions).HasMaxLength(500);

        builder.Property(item => item.SortOrder).IsRequired();
    }
}
