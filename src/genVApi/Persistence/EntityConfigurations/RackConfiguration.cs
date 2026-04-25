using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.EntityConfigurations;

public class RackConfiguration : IEntityTypeConfiguration<Rack>
{
    public void Configure(EntityTypeBuilder<Rack> builder)
    {
        builder.ToTable("Racks").HasKey(r => r.Id);

        builder.Property(r => r.Id).HasColumnName("Id").IsRequired();
        builder.Property(r => r.TankId).HasColumnName("TankId").IsRequired();
        builder.Property(r => r.Name).HasColumnName("Name").HasMaxLength(100).IsRequired();
        builder.Property(r => r.CreatedDate).HasColumnName("CreatedDate").IsRequired();
        builder.Property(r => r.UpdatedDate).HasColumnName("UpdatedDate");
        builder.Property(r => r.DeletedDate).HasColumnName("DeletedDate");

        builder.HasMany(r => r.Slots)
            .WithOne(s => s.Rack)
            .HasForeignKey(s => s.RackId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(r => new { r.TankId, r.Name }).IsUnique();
    }
}
