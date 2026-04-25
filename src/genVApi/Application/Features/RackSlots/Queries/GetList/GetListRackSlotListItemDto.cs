using NArchitecture.Core.Application.Dtos;

namespace Application.Features.RackSlots.Queries.GetList;

public class GetListRackSlotListItemDto : IDto
{
    public Guid Id { get; set; }
    public Guid RackId { get; set; }
    public string Name { get; set; } = default!;
    public DateTime CreatedDate { get; set; }
}
