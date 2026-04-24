using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.EntityConfigurations;

public class BagConfiguration : IEntityTypeConfiguration<Bag>
{
    public void Configure(EntityTypeBuilder<Bag> builder)
    {
        builder.ToTable("Bags").HasKey(b => b.Id);

        builder.Property(b => b.Id).HasColumnName("Id").IsRequired();
        builder.Property(b => b.SessionId).HasColumnName("SessionId").IsRequired();
        builder.Property(b => b.BagNumber).HasColumnName("BagNumber").IsRequired();
        builder.Property(b => b.VolumeMl).HasColumnName("VolumeMl").IsRequired();
        builder.Property(b => b.SourceVolumeMl).HasColumnName("SourceVolumeMl").IsRequired();
        builder.Property(b => b.Wbc).HasColumnName("Wbc");
        builder.Property(b => b.Cd34Percent).HasColumnName("Cd34Percent");
        builder.Property(b => b.Cd45Percent).HasColumnName("Cd45Percent");
        builder.Property(b => b.Cd3Percent).HasColumnName("Cd3Percent");
        builder.Property(b => b.CompositionNote).HasColumnName("CompositionNote").HasMaxLength(120);
        builder.Property(b => b.Cd34PerKg).HasColumnName("Cd34PerKg").IsRequired();
        builder.Property(b => b.Cd3PerKg).HasColumnName("Cd3PerKg").IsRequired();
        builder.Property(b => b.Status).HasColumnName("Status").HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(b => b.Purpose).HasColumnName("Purpose").HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(b => b.SplitBatchId).HasColumnName("SplitBatchId");
        builder.Property(b => b.SlotId).HasColumnName("SlotId");

        builder.Property(b => b.CreatedDate).HasColumnName("CreatedDate").IsRequired();
        builder.Property(b => b.UpdatedDate).HasColumnName("UpdatedDate");
        builder.Property(b => b.DeletedDate).HasColumnName("DeletedDate");

        builder.HasOne(b => b.Slot)
               .WithOne(s => s.Bag!)
               .HasForeignKey<Bag>(b => b.SlotId)
               .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(b => b.SlotId).IsUnique().HasFilter("\"SlotId\" IS NOT NULL");

        builder.HasQueryFilter(b => !b.DeletedDate.HasValue);
    }
}
