using NArchitecture.Core.Application.Dtos;

namespace Application.Features.CollectionSessions.Queries.GetList;

public class GetListCollectionSessionListItemDto : IDto
{
    public Guid Id { get; set; }
    public Guid PatientId { get; set; }
    public int Day { get; set; }
    public DateTime Date { get; set; }
    public double? WbcPre { get; set; }
    public double? Hgb { get; set; }
    public double? Hct { get; set; }
    public double? Plt { get; set; }
    public double? PreCd45Percent { get; set; }
    public double? PreCd34Percent { get; set; }
    public double? PreMhs { get; set; }
    public double? WbcPost { get; set; }
    public double? HgbPost { get; set; }
    public double? HctPost { get; set; }
    public double? PltPost { get; set; }
    public double VolumeMl { get; set; }
    public double WBC { get; set; }
    public double Cd34Percent { get; set; }
    public double Cd45Percent { get; set; }
    public double Cd3Percent { get; set; }
    public double? LymphocytePercent { get; set; }
    public double? Mhs { get; set; }
    public double Cd34PerKg { get; set; }
    public double Cd3PerKg { get; set; }
    public DateTime CreatedDate { get; set; }
}