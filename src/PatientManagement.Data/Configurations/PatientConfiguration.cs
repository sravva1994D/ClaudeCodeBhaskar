using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PatientManagement.Data.Entities;

namespace PatientManagement.Data.Configurations;

// Column rules and indexes from Documentation/02-Planning/07-Database-Design.md §2 and §4.
public class PatientConfiguration : IEntityTypeConfiguration<Patient>
{
    // "PT-" then the identity, zero-padded to six digits. Beyond 999,999 the full number is used, so codes never repeat.
    private const string PatientCodeSql =
        "'PT-' + CASE WHEN [Id] < 1000000 THEN RIGHT('000000' + CAST([Id] AS varchar(10)), 6) ELSE CAST([Id] AS varchar(10)) END";

    public void Configure(EntityTypeBuilder<Patient> builder)
    {
        builder.ToTable("Patients", table =>
            table.HasCheckConstraint("CK_Patients_Gender", "[Gender] IN (N'Male', N'Female', N'Other')"));

        builder.HasKey(patient => patient.Id);

        builder.Property(patient => patient.PatientCode)
            .HasMaxLength(20)
            .HasComputedColumnSql(PatientCodeSql, stored: true)
            .ValueGeneratedOnAddOrUpdate();
        builder.HasIndex(patient => patient.PatientCode).IsUnique();

        builder.Property(patient => patient.FullName).HasMaxLength(200).IsRequired();
        builder.HasIndex(patient => patient.FullName);

        builder.Property(patient => patient.DateOfBirth).IsRequired();

        builder.Property(patient => patient.Gender).HasMaxLength(20).IsRequired();

        builder.Property(patient => patient.Phone).HasMaxLength(20).IsRequired();
        builder.HasIndex(patient => patient.Phone);

        builder.HasIndex(patient => new { patient.DateOfBirth, patient.Phone });

        builder.Property(patient => patient.Email).HasMaxLength(256).IsRequired();

        builder.Property(patient => patient.Address).HasMaxLength(500).IsRequired();
    }
}
