using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PatientManagement.Data.Entities;

namespace PatientManagement.Data.Configurations;

// Column rules from Documentation/02-Planning/07-Database-Design.md §2 (Users).
public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");

        builder.HasKey(user => user.Id);

        builder.Property(user => user.Username).HasMaxLength(50).IsRequired();
        builder.HasIndex(user => user.Username).IsUnique();

        builder.Property(user => user.PasswordHash).HasMaxLength(256).IsRequired();

        builder.Property(user => user.Email).HasMaxLength(256).IsRequired();
    }
}
