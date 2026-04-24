namespace Application.Features.ClinicalConfiguration;

public class ClinicalSettingsDto
{
    public Guid Id { get; set; }
    public double SessionCd34Cd3Divisor { get; set; }
    public double DliCd3CalculationDivisor { get; set; }
    public double TargetCd34AutologousPerKg { get; set; }
    public double TargetCd34AllogeneicPerKg { get; set; }
    public double IdealCd34AutologousPerKg { get; set; }
    public double IdealCd34AllogeneicPerKg { get; set; }
    public int MaxApheresisDaysAutologous { get; set; }
    public int MaxApheresisDaysAllogeneic { get; set; }
    public double DliHighDoseCd3PerKgThreshold { get; set; }
    public double ProductMinimumCd34PerKg { get; set; }
    public double DashboardCd34LowThreshold { get; set; }
    public double DashboardCd34ElevatedFloor { get; set; }
    public double DashboardCd3HighRiskThreshold { get; set; }
    public double DashboardCd3LowImmuneThreshold { get; set; }
    public double DashboardCd3OptimalMin { get; set; }
    public double DashboardCd3OptimalMax { get; set; }
}
