using NArchitecture.Core.Application.Responses;

namespace Application.Features.Products.Queries.GetById;

public class GetByIdProductResponse : IResponse
{
    public Guid Id { get; set; }
    public Guid SessionId { get; set; }
    public double TotalVolume { get; set; }
    public double TotalWbc { get; set; }
    public double Cd34Percent { get; set; }
    public double Cd45Percent { get; set; }
    public double Cd3Percent { get; set; }
    public double TotalCd34PerKg { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime? UpdatedDate { get; set; }
}