using NArchitecture.Core.Application.Dtos;

namespace Application.Features.Bags.Queries.GetList;

public class GetListBagListItemDto : IDto
{
    public Guid Id { get; set; }
    public Guid SessionId { get; set; }
    public int BagNumber { get; set; }
    public double VolumeMl { get; set; }
    public double SourceVolumeMl { get; set; }
    public double? Wbc { get; set; }
    public double? Cd34Percent { get; set; }
    public double? Cd45Percent { get; set; }
    public double? Cd3Percent { get; set; }
    public string? CompositionNote { get; set; }
    public double Cd34PerKg { get; set; }
    public double Cd3PerKg { get; set; }
    public Domain.Enums.BagStatus Status { get; set; }
    public Domain.Enums.BagUseReason? UseReason { get; set; }
    public string? UseNote { get; set; }
    public Domain.Enums.BagPurpose Purpose { get; set; }
    public Guid? SplitBatchId { get; set; }
    public Guid? BagCellId { get; set; }
    public DateTime CreatedDate { get; set; }
}