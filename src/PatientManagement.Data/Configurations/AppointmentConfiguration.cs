using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PatientManagement.Data.Entities;

namespace PatientManagement.Data.Configurations;

// Column rules and indexes from Documentation/02-Planning/07-Database-Design.md §2 (Appointments) and §4.
public class AppointmentConfiguration : IEntityTypeConfiguration<Appointment>
{
    public void Configure(EntityTypeBuilder<Appointment> builder)
    {
        builder.ToTable("Appointments", table =>
        {
            table.HasCheckConstraint("CK_Appointments_DurationMinutes", "[DurationMinutes] = 30");
            table.HasCheckConstraint(
                "CK_Appointments_Status",
                "[Status] IN (N'Scheduled', N'Checked-In', N'In Consultation', N'Completed', N'Cancelled', N'No-show')");
        });

        builder.HasKey(appointment => appointment.Id);

        builder.HasOne<Patient>()
            .WithMany()
            .HasForeignKey(appointment => appointment.PatientId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(appointment => appointment.StartTime).HasColumnType("datetime2(0)").IsRequired();

        builder.Property(appointment => appointment.DurationMinutes).HasDefaultValue((short)30).IsRequired();

        builder.Property(appointment => appointment.Status).HasMaxLength(20).IsRequired();

        builder.Property(appointment => appointment.IsArchived).HasDefaultValue(false).IsRequired();

        // Daily list (FR-APT-002): filter by date, exclude archived.
        builder.HasIndex(appointment => new { appointment.StartTime, appointment.IsArchived })
            .HasDatabaseName("IX_Appointments_StartTime");

        // DB-level no-overlap guard (PDR-07). Only one live appointment may hold a slot start.
        // Cancelled and No-show release the slot, so they are outside the filter.
        // Written with <> rather than IN, because SQL Server filtered indexes do not accept IN.
        builder.HasIndex(appointment => appointment.StartTime)
            .IsUnique()
            .HasDatabaseName("IX_Appointments_Slot")
            .HasFilter("[IsArchived] = 0 AND [Status] <> N'Cancelled' AND [Status] <> N'No-show'");
    }
}
