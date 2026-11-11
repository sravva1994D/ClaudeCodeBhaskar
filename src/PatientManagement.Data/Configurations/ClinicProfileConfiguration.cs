using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PatientManagement.Data.Entities;

namespace PatientManagement.Data.Configurations;

// Column rules from Documentation/02-Planning/07-Database-Design.md §2 (ClinicProfile) and §5 (CHECK Id = 1).
public class ClinicProfileConfiguration : IEntityTypeConfiguration<ClinicProfile>
{
    public void Configure(EntityTypeBuilder<ClinicProfile> builder)
    {
        builder.ToTable("ClinicProfile", table =>
        {
            table.HasCheckConstraint("CK_ClinicProfile_Id", "[Id] = 1");
        });

        builder.HasKey(profile => profile.Id);

        // The single row is always Id = 1, set by the application (PDR-15).
        builder.Property(profile => profile.Id).ValueGeneratedNever();

        builder.Property(profile => profile.ClinicName).HasMaxLength(200);
        builder.Property(profile => profile.Address).HasMaxLength(500);
        builder.Property(profile => profile.Phone).HasMaxLength(20);
        builder.Property(profile => profile.DoctorName).HasMaxLength(200);
        builder.Property(profile => profile.RegistrationNumber).HasMaxLength(100);
    }
}
