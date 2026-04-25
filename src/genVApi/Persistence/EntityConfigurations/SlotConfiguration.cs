using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.EntityConfigurations;

/// <summary>Raf üzeri slot — tablo adı <c>RackSlots</c> (eski <c>Slots</c> tablosu torba hücresi için <c>BagCells</c> oldu).</summary>
public class SlotConfiguration : IEntityTypeConfiguration<Slot>
{
    public void Configure(EntityTypeBuilder<Slot> builder)
    {
        builder.ToTable("RackSlots").HasKey(s => s.Id);

        builder.Property(s => s.Id).HasColumnName("Id").IsRequired();
        builder.Property(s => s.RackId).HasColumnName("RackId").IsRequired();
        builder.Property(s => s.Name).HasColumnName("Name").HasMaxLength(100).IsRequired();
        builder.Property(s => s.CreatedDate).HasColumnName("CreatedDate").IsRequired();
        builder.Property(s => s.UpdatedDate).HasColumnName("UpdatedDate");
        builder.Property(s => s.DeletedDate).HasColumnName("DeletedDate");

        builder.HasMany(s => s.Boxes)
            .WithOne(b => b.Slot)
            .HasForeignKey(b => b.SlotId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(s => new { s.RackId, s.Name }).IsUnique();
    }
}
