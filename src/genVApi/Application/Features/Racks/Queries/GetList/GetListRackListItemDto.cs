using NArchitecture.Core.Application.Dtos;

namespace Application.Features.Racks.Queries.GetList;

public class GetListRackListItemDto : IDto
{
    public Guid Id { get; set; }
    public Guid TankId { get; set; }
    public string Name { get; set; }
    public DateTime CreatedDate { get; set; }
}