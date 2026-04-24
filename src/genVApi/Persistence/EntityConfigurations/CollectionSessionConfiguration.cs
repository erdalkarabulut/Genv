using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.EntityConfigurations;

public class CollectionSessionConfiguration : IEntityTypeConfiguration<CollectionSession>
{
    public void Configure(EntityTypeBuilder<CollectionSession> builder)
    {
        builder.ToTable("CollectionSessions").HasKey(cs => cs.Id);

        builder.Property(cs => cs.Id).HasColumnName("Id").IsRequired();
        builder.Property(cs => cs.PatientId).HasColumnName("PatientId").IsRequired();
        builder.Property(cs => cs.Day).HasColumnName("Day").IsRequired();
        builder.Property(cs => cs.Date).HasColumnName("Date").IsRequired();

        builder.Property(cs => cs.WbcPre).HasColumnName("WbcPre");
        builder.Property(cs => cs.Hgb).HasColumnName("Hgb");
        builder.Property(cs => cs.Hct).HasColumnName("Hct");
        builder.Property(cs => cs.Plt).HasColumnName("Plt");

        builder.Property(cs => cs.PreCd45Percent).HasColumnName("PreCd45Percent");
        builder.Property(cs => cs.PreCd34Percent).HasColumnName("PreCd34Percent");
        builder.Property(cs => cs.PreMhs).HasColumnName("PreMhs");

        builder.Property(cs => cs.WbcPost).HasColumnName("WbcPost");
        builder.Property(cs => cs.HgbPost).HasColumnName("HgbPost");
        builder.Property(cs => cs.HctPost).HasColumnName("HctPost");
        builder.Property(cs => cs.PltPost).HasColumnName("PltPost");

        builder.Property(cs => cs.VolumeMl).HasColumnName("VolumeMl").IsRequired();
        builder.Property(cs => cs.WBC).HasColumnName("WBC").IsRequired();
        builder.Property(cs => cs.Cd34Percent).HasColumnName("Cd34Percent").IsRequired();
        builder.Property(cs => cs.Cd45Percent).HasColumnName("Cd45Percent").IsRequired();
        builder.Property(cs => cs.Cd3Percent).HasColumnName("Cd3Percent").IsRequired();
        builder.Property(cs => cs.LymphocytePercent).HasColumnName("LymphocytePercent");
        builder.Property(cs => cs.Mhs).HasColumnName("Mhs");
        builder.Property(cs => cs.Cd34PerKg).HasColumnName("Cd34PerKg").IsRequired();
        builder.Property(cs => cs.Cd3PerKg).HasColumnName("Cd3PerKg").IsRequired();

        builder.Property(cs => cs.CreatedDate).HasColumnName("CreatedDate").IsRequired();
        builder.Property(cs => cs.UpdatedDate).HasColumnName("UpdatedDate");
        builder.Property(cs => cs.DeletedDate).HasColumnName("DeletedDate");

        builder.HasMany(cs => cs.Bags)
               .WithOne(b => b.Session)
               .HasForeignKey(b => b.SessionId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(cs => cs.Product)
               .WithOne(p => p.Session)
               .HasForeignKey<Product>(p => p.SessionId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasQueryFilter(cs => !cs.DeletedDate.HasValue);
    }
}
