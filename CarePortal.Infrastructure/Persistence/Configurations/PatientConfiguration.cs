using CarePortal.Domain.Patients;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CarePortal.Infrastructure.Persistence.Configurations;

public sealed class PatientConfiguration : IEntityTypeConfiguration<Patient>
{
    public void Configure(EntityTypeBuilder<Patient> builder)
    {
        builder.ToTable("patients");

        builder.HasKey(patient => patient.Id);

        builder.Property(patient => patient.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(patient => patient.FirstName)
            .HasColumnName("first_name")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(patient => patient.LastName)
            .HasColumnName("last_name")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(patient => patient.Email)
            .HasColumnName("email")
            .HasMaxLength(256)
            .IsRequired();

        builder.HasIndex(patient => patient.Email)
            .IsUnique();

        builder.Property(patient => patient.DateOfBirth)
            .HasColumnName("date_of_birth")
            .IsRequired();

        builder.Property(patient => patient.CreatedAtUtc)
            .HasColumnName("created_at_utc")
            .IsRequired();

        builder.Ignore(patient => patient.DomainEvents);
    }
}
