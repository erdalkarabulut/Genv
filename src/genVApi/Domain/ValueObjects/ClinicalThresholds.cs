using Domain.Entities;

namespace Domain.ValueObjects;

/// <summary>
/// Klinik eşiklerin hesap için kullanılan immutable görünümü (<see cref="ClinicalSettings"/> ile aynı değerler).
/// </summary>
public readonly record struct ClinicalThresholds(
    double SessionCd34Cd3Divisor,
    double DliCd3CalculationDivisor,
    double TargetCd34AutologousPerKg,
    double TargetCd34AllogeneicPerKg,
    double IdealCd34AutologousPerKg,
    double IdealCd34AllogeneicPerKg,
    int MaxApheresisDaysAutologous,
    int MaxApheresisDaysAllogeneic,
    double DliHighDoseCd3PerKgThreshold,
    double ProductMinimumCd34PerKg,
    double DashboardCd34LowThreshold,
    double DashboardCd34ElevatedFloor,
    double DashboardCd3HighRiskThreshold,
    double DashboardCd3LowImmuneThreshold,
    double DashboardCd3OptimalMin,
    double DashboardCd3OptimalMax)
{
    public static ClinicalThresholds Default { get; } = new(
        SessionCd34Cd3Divisor: 10000,
        DliCd3CalculationDivisor: 10000,
        TargetCd34AutologousPerKg: 2,
        TargetCd34AllogeneicPerKg: 4,
        IdealCd34AutologousPerKg: 4,
        IdealCd34AllogeneicPerKg: 5,
        MaxApheresisDaysAutologous: 4,
        MaxApheresisDaysAllogeneic: 2,
        DliHighDoseCd3PerKgThreshold: 10,
        ProductMinimumCd34PerKg: 2,
        DashboardCd34LowThreshold: 2,
        DashboardCd34ElevatedFloor: 4,
        DashboardCd3HighRiskThreshold: 10,
        DashboardCd3LowImmuneThreshold: 2,
        DashboardCd3OptimalMin: 3,
        DashboardCd3OptimalMax: 8);

    public static ClinicalThresholds FromEntity(ClinicalSettings s) => new(
        s.SessionCd34Cd3Divisor,
        s.DliCd3CalculationDivisor,
        s.TargetCd34AutologousPerKg,
        s.TargetCd34AllogeneicPerKg,
        s.IdealCd34AutologousPerKg,
        s.IdealCd34AllogeneicPerKg,
        s.MaxApheresisDaysAutologous,
        s.MaxApheresisDaysAllogeneic,
        s.DliHighDoseCd3PerKgThreshold,
        s.ProductMinimumCd34PerKg,
        s.DashboardCd34LowThreshold,
        s.DashboardCd34ElevatedFloor,
        s.DashboardCd3HighRiskThreshold,
        s.DashboardCd3LowImmuneThreshold,
        s.DashboardCd3OptimalMin,
        s.DashboardCd3OptimalMax);

    public double ComputeSessionCd34PerKg(
        double volumeMl,
        double wbc,
        double cd45Percent,
        double cd34Percent,
        double weightKg)
    {
        double total = volumeMl * wbc;
        return (total * (cd45Percent ) * (cd34Percent)) / SessionCd34Cd3Divisor / weightKg;
    }

    public double ComputeSessionCd3PerKg(double volumeMl, double wbc, double cd3Percent, double weightKg)
    {
        double total = volumeMl * wbc;
        return (total * (cd3Percent )) / SessionCd34Cd3Divisor / weightKg;
    }

    public string ComputeAggregateRisk(double totalCd34, double totalCd3)
    {
        if (totalCd34 < DashboardCd34LowThreshold)
            return "Yetersiz";
        if (totalCd34 >= DashboardCd34ElevatedFloor && totalCd3 > DashboardCd3HighRiskThreshold)
            return "GVHD Riski";
        if (totalCd34 >= DashboardCd34ElevatedFloor && totalCd3 < DashboardCd3LowImmuneThreshold)
            return "Düşük Bağışıklık";
        if (totalCd34 >= DashboardCd34ElevatedFloor
            && totalCd3 >= DashboardCd3OptimalMin
            && totalCd3 <= DashboardCd3OptimalMax)
            return "Optimal";
        return "Normal";
    }
}
