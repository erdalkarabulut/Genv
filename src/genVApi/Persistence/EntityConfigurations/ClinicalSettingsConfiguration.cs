using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.EntityConfigurations;

public class ClinicalSettingsConfiguration : IEntityTypeConfiguration<ClinicalSettings>
{
    public void Configure(EntityTypeBuilder<ClinicalSettings> builder)
    {
        builder.ToTable("ClinicalSettings").HasKey(x => x.Id);

        builder.Property(x => x.Id).HasColumnName("Id");
        builder.Property(x => x.SessionCd34Cd3Divisor).HasColumnName("SessionCd34Cd3Divisor").IsRequired();
        builder.Property(x => x.DliCd3CalculationDivisor).HasColumnName("DliCd3CalculationDivisor").IsRequired();
        builder.Property(x => x.TargetCd34AutologousPerKg).HasColumnName("TargetCd34AutologousPerKg").IsRequired();
        builder.Property(x => x.TargetCd34AllogeneicPerKg).HasColumnName("TargetCd34AllogeneicPerKg").IsRequired();
        builder.Property(x => x.IdealCd34AutologousPerKg).HasColumnName("IdealCd34AutologousPerKg").IsRequired();
        builder.Property(x => x.IdealCd34AllogeneicPerKg).HasColumnName("IdealCd34AllogeneicPerKg").IsRequired();
        builder.Property(x => x.MaxApheresisDaysAutologous).HasColumnName("MaxApheresisDaysAutologous").IsRequired();
        builder.Property(x => x.MaxApheresisDaysAllogeneic).HasColumnName("MaxApheresisDaysAllogeneic").IsRequired();
        builder.Property(x => x.DliHighDoseCd3PerKgThreshold).HasColumnName("DliHighDoseCd3PerKgThreshold").IsRequired();
        builder.Property(x => x.ProductMinimumCd34PerKg).HasColumnName("ProductMinimumCd34PerKg").IsRequired();
        builder.Property(x => x.DashboardCd34LowThreshold).HasColumnName("DashboardCd34LowThreshold").IsRequired();
        builder.Property(x => x.DashboardCd34ElevatedFloor).HasColumnName("DashboardCd34ElevatedFloor").IsRequired();
        builder.Property(x => x.DashboardCd3HighRiskThreshold).HasColumnName("DashboardCd3HighRiskThreshold").IsRequired();
        builder.Property(x => x.DashboardCd3LowImmuneThreshold).HasColumnName("DashboardCd3LowImmuneThreshold").IsRequired();
        builder.Property(x => x.DashboardCd3OptimalMin).HasColumnName("DashboardCd3OptimalMin").IsRequired();
        builder.Property(x => x.DashboardCd3OptimalMax).HasColumnName("DashboardCd3OptimalMax").IsRequired();

        builder.Property(x => x.CreatedDate).HasColumnName("CreatedDate").IsRequired();
        builder.Property(x => x.UpdatedDate).HasColumnName("UpdatedDate");
        builder.Property(x => x.DeletedDate).HasColumnName("DeletedDate");
    }
}
