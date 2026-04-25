using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.EntityConfigurations;

public class BagCellConfiguration : IEntityTypeConfiguration<BagCell>
{
    public void Configure(EntityTypeBuilder<BagCell> builder)
    {
        builder.ToTable("BagCells").HasKey(c => c.Id);

        builder.Property(c => c.Id).HasColumnName("Id").IsRequired();
        builder.Property(c => c.BoxId).HasColumnName("BoxId").IsRequired();
        builder.Property(c => c.Position).HasColumnName("Position").HasMaxLength(20).IsRequired();
        builder.Property(c => c.IsOccupied).HasColumnName("IsOccupied").IsRequired();
        builder.Property(c => c.Version).HasColumnName("Version").IsConcurrencyToken().HasDefaultValue(0).IsRequired();
        builder.Property(c => c.CreatedDate).HasColumnName("CreatedDate").IsRequired();
        builder.Property(c => c.UpdatedDate).HasColumnName("UpdatedDate");
        builder.Property(c => c.DeletedDate).HasColumnName("DeletedDate");

        builder.HasIndex(c => new { c.BoxId, c.Position }).IsUnique();
    }
}
