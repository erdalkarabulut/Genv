using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.EntityConfigurations;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("Products").HasKey(p => p.Id);

        builder.Property(p => p.Id).HasColumnName("Id").IsRequired();
        builder.Property(p => p.SessionId).HasColumnName("SessionId").IsRequired();
        builder.Property(p => p.TotalVolume).HasColumnName("TotalVolume").IsRequired();
        builder.Property(p => p.TotalWbc).HasColumnName("TotalWbc").IsRequired();
        builder.Property(p => p.Cd34Percent).HasColumnName("Cd34Percent").IsRequired();
        builder.Property(p => p.Cd45Percent).HasColumnName("Cd45Percent").IsRequired();
        builder.Property(p => p.Cd3Percent).HasColumnName("Cd3Percent").IsRequired();
        builder.Property(p => p.TotalCd34PerKg).HasColumnName("TotalCd34PerKg").IsRequired();

        builder.Property(p => p.CreatedDate).HasColumnName("CreatedDate").IsRequired();
        builder.Property(p => p.UpdatedDate).HasColumnName("UpdatedDate");
        builder.Property(p => p.DeletedDate).HasColumnName("DeletedDate");

        builder.HasIndex(p => p.SessionId).IsUnique();

        builder.HasQueryFilter(p => !p.DeletedDate.HasValue);
    }
}
