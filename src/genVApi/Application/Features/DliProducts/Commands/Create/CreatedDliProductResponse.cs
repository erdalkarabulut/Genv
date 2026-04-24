using NArchitecture.Core.Application.Responses;

namespace Application.Features.DliProducts.Commands.Create;

public class CreatedDliProductResponse : IResponse
{
    public Guid Id { get; set; }
    public Guid PatientId { get; set; }
    public Guid? SessionId { get; set; }
    public Guid? DonorId { get; set; }
    public DateTime Date { get; set; }
    public double VolumeMl { get; set; }
    public double? Wbc { get; set; }
    public double? LymphocytePercent { get; set; }
    public double? Cd3Percent { get; set; }
    public double TotalCd3 { get; set; }
    public double Cd3PerKg { get; set; }
    public string? Notes { get; set; }
}