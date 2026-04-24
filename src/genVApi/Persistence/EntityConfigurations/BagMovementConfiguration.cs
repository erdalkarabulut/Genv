using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.EntityConfigurations;

public class BagMovementConfiguration : IEntityTypeConfiguration<BagMovement>
{
    public void Configure(EntityTypeBuilder<BagMovement> builder)
    {
        builder.ToTable("BagMovements").HasKey(bm => bm.Id);

        builder.Property(bm => bm.Id).HasColumnName("Id").IsRequired();
        builder.Property(bm => bm.BagId).HasColumnName("BagId").IsRequired();
        builder.Property(bm => bm.FromSlotId).HasColumnName("FromSlotId");
        builder.Property(bm => bm.ToSlotId).HasColumnName("ToSlotId");
        builder.Property(bm => bm.Action).HasColumnName("Action").HasMaxLength(50).IsRequired();

        builder.Property(bm => bm.CreatedDate).HasColumnName("CreatedDate").IsRequired();
        builder.Property(bm => bm.UpdatedDate).HasColumnName("UpdatedDate");
        builder.Property(bm => bm.DeletedDate).HasColumnName("DeletedDate");

        builder.HasOne(bm => bm.Bag)
               .WithMany()
               .HasForeignKey(bm => bm.BagId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasQueryFilter(bm => !bm.DeletedDate.HasValue);
    }
}
