using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.EntityConfigurations;

public class SlotConfiguration : IEntityTypeConfiguration<Slot>
{
    public void Configure(EntityTypeBuilder<Slot> builder)
    {
        builder.ToTable("Slots").HasKey(s => s.Id);

        builder.Property(s => s.Id).HasColumnName("Id").IsRequired();
        builder.Property(s => s.BoxId).HasColumnName("BoxId").IsRequired();
        builder.Property(s => s.Position).HasColumnName("Position").HasMaxLength(20).IsRequired();
        builder.Property(s => s.IsOccupied).HasColumnName("IsOccupied").IsRequired();
        builder.Property(s => s.Version).HasColumnName("Version").IsConcurrencyToken().HasDefaultValue(0).IsRequired();
        builder.Property(s => s.CreatedDate).HasColumnName("CreatedDate").IsRequired();
        builder.Property(s => s.UpdatedDate).HasColumnName("UpdatedDate");
        builder.Property(s => s.DeletedDate).HasColumnName("DeletedDate");

        builder.HasIndex(s => new { s.BoxId, s.Position }).IsUnique();

        builder.HasQueryFilter(s => !s.DeletedDate.HasValue);
    }
}
