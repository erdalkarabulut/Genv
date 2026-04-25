using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.EntityConfigurations;

public class PatientConfiguration : IEntityTypeConfiguration<Patient>
{
    public void Configure(EntityTypeBuilder<Patient> builder)
    {
        builder.ToTable("Patients").HasKey(p => p.Id);

        builder.Property(p => p.Id).HasColumnName("Id").IsRequired();
        builder.Property(p => p.FullName).HasColumnName("FullName").HasMaxLength(200).IsRequired();
        builder.Property(p => p.WeightKg).HasColumnName("WeightKg").IsRequired();
        builder.Property(p => p.BloodGroup).HasColumnName("BloodGroup").HasMaxLength(10);
        builder.Property(p => p.TransplantType).HasColumnName("TransplantType").HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(p => p.Diagnosis).HasColumnName("Diagnosis").HasMaxLength(500);
        builder.Property(p => p.ProtocolNo).HasColumnName("ProtocolNo").HasMaxLength(100);
        builder.Property(p => p.BirthDate).HasColumnName("BirthDate");
        builder.Property(p => p.DonorId).HasColumnName("DonorId");
        builder.Property(p => p.CreatedDate).HasColumnName("CreatedDate").IsRequired();
        builder.Property(p => p.UpdatedDate).HasColumnName("UpdatedDate");
        builder.Property(p => p.DeletedDate).HasColumnName("DeletedDate");

        builder.HasOne(p => p.Donor)
               .WithMany()
               .HasForeignKey(p => p.DonorId)
               .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(p => p.Sessions)
               .WithOne(s => s.Patient)
               .HasForeignKey(s => s.PatientId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(p => p.DliProducts)
               .WithOne(d => d.Patient)
               .HasForeignKey(d => d.PatientId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
