using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.EntityConfigurations;

public class BoxConfiguration : IEntityTypeConfiguration<Box>
{
    public void Configure(EntityTypeBuilder<Box> builder)
    {
        builder.ToTable("Boxes").HasKey(b => b.Id);

        builder.Property(b => b.Id).HasColumnName("Id").IsRequired();
        builder.Property(b => b.RackId).HasColumnName("RackId").IsRequired();
        builder.Property(b => b.Name).HasColumnName("Name").HasMaxLength(100).IsRequired();
        builder.Property(b => b.CreatedDate).HasColumnName("CreatedDate").IsRequired();
        builder.Property(b => b.UpdatedDate).HasColumnName("UpdatedDate");
        builder.Property(b => b.DeletedDate).HasColumnName("DeletedDate");

        builder.HasMany(b => b.Slots)
               .WithOne(s => s.Box)
               .HasForeignKey(s => s.BoxId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(b => new { b.RackId, b.Name }).IsUnique();

        builder.HasQueryFilter(b => !b.DeletedDate.HasValue);
    }
}
