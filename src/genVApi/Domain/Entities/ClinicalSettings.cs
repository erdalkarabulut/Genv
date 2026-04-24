using NArchitecture.Core.Persistence.Repositories;

namespace Domain.Entities;

/// <summary>
/// Tek satırlık sistem ayarı: CD34/CD3 hesap bölenleri ve kabul eşikleri (admin tarafından güncellenir).
/// </summary>
public class ClinicalSettings : Entity<Guid>
{
    /// <summary>Sabit kimlik — tabloda tek kayıt.</summary>
    public static readonly Guid SingletonId = Guid.Parse("11111111-1111-1111-1111-111111111111");

    /// <summary>Aferez seansı CD34 ve CD3/kg formülündeki bölen (varsayılan 10 000).</summary>
    public double SessionCd34Cd3Divisor { get; set; } = 10000;

    /// <summary>DLI toplam CD3 hesabındaki bölen (varsayılan 10 000).</summary>
    public double DliCd3CalculationDivisor { get; set; } = 10000;

    public double TargetCd34AutologousPerKg { get; set; } = 2;
    public double TargetCd34AllogeneicPerKg { get; set; } = 4;
    public double IdealCd34AutologousPerKg { get; set; } = 4;
    public double IdealCd34AllogeneicPerKg { get; set; } = 5;

    public int MaxApheresisDaysAutologous { get; set; } = 4;
    public int MaxApheresisDaysAllogeneic { get; set; } = 2;

    /// <summary>DLI &quot;yüksek doz&quot; eşiği (×10⁶/kg CD3).</summary>
    public double DliHighDoseCd3PerKgThreshold { get; set; } = 10;

    /// <summary>Ürün yeterlilik kontrolü için minimum CD34/kg.</summary>
    public double ProductMinimumCd34PerKg { get; set; } = 2;

    public double DashboardCd34LowThreshold { get; set; } = 2;
    public double DashboardCd34ElevatedFloor { get; set; } = 4;
    public double DashboardCd3HighRiskThreshold { get; set; } = 10;
    public double DashboardCd3LowImmuneThreshold { get; set; } = 2;
    public double DashboardCd3OptimalMin { get; set; } = 3;
    public double DashboardCd3OptimalMax { get; set; } = 8;
}
