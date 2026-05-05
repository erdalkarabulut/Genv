using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.EntityConfigurations;

public class DonorConfiguration : IEntityTypeConfiguration<Donor>
{
    public void Configure(EntityTypeBuilder<Donor> builder)
    {
        builder.ToTable("Donors").HasKey(d => d.Id);

        builder.Property(d => d.Id).HasColumnName("Id").IsRequired();
        builder.Property(d => d.FullName).HasColumnName("FullName").HasMaxLength(200).IsRequired();
        builder.Property(d => d.WeightKg).HasColumnName("WeightKg").IsRequired();
        builder.Property(d => d.BloodGroup).HasColumnName("BloodGroup").HasMaxLength(10);
        builder.Property(d => d.Relation).HasColumnName("Relation").HasMaxLength(100);
        builder.Property(d => d.IdentityNumber).HasColumnName("IdentityNumber").HasMaxLength(20);
        builder.Property(d => d.DonorType).HasColumnName("DonorType").HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(d => d.BirthDate).HasColumnName("BirthDate");
        builder.Property(d => d.CreatedDate).HasColumnName("CreatedDate").IsRequired();
        builder.Property(d => d.UpdatedDate).HasColumnName("UpdatedDate");
        builder.Property(d => d.DeletedDate).HasColumnName("DeletedDate");
    }
}
