using NArchitecture.Core.Application.Dtos;

namespace Application.Features.Slots.Queries.GetList;

public class GetListSlotListItemDto : IDto
{
    public Guid Id { get; set; }
    public Guid BoxId { get; set; }
    public string Position { get; set; }
    public bool IsOccupied { get; set; }
    public int Version { get; set; }
    public DateTime CreatedDate { get; set; }
}