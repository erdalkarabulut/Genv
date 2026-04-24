using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.EntityConfigurations;

public class DliProductConfiguration : IEntityTypeConfiguration<DliProduct>
{
    public void Configure(EntityTypeBuilder<DliProduct> builder)
    {
        builder.ToTable("DliProducts").HasKey(dp => dp.Id);

        builder.Property(dp => dp.Id).HasColumnName("Id").IsRequired();
        builder.Property(dp => dp.PatientId).HasColumnName("PatientId").IsRequired();
        builder.Property(dp => dp.SessionId).HasColumnName("SessionId");
        builder.Property(dp => dp.DonorId).HasColumnName("DonorId");
        builder.Property(dp => dp.Date).HasColumnName("Date").IsRequired();
        builder.Property(dp => dp.VolumeMl).HasColumnName("VolumeMl").IsRequired();
        builder.Property(dp => dp.Wbc).HasColumnName("Wbc");
        builder.Property(dp => dp.LymphocytePercent).HasColumnName("LymphocytePercent");
        builder.Property(dp => dp.Cd3Percent).HasColumnName("Cd3Percent");
        builder.Property(dp => dp.TotalCd3).HasColumnName("TotalCd3").IsRequired();
        builder.Property(dp => dp.Cd3PerKg).HasColumnName("Cd3PerKg").IsRequired();
        builder.Property(dp => dp.Notes).HasColumnName("Notes").HasMaxLength(1000);

        builder.Property(dp => dp.CreatedDate).HasColumnName("CreatedDate").IsRequired();
        builder.Property(dp => dp.UpdatedDate).HasColumnName("UpdatedDate");
        builder.Property(dp => dp.DeletedDate).HasColumnName("DeletedDate");

        builder.HasOne(dp => dp.Patient)
               .WithMany(p => p.DliProducts)
               .HasForeignKey(dp => dp.PatientId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(dp => dp.Session)
               .WithMany()
               .HasForeignKey(dp => dp.SessionId)
               .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(dp => dp.Donor)
               .WithMany()
               .HasForeignKey(dp => dp.DonorId)
               .OnDelete(DeleteBehavior.SetNull);

        builder.HasQueryFilter(dp => !dp.DeletedDate.HasValue);
    }
}
