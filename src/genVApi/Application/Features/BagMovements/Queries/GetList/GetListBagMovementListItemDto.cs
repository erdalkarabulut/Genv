using NArchitecture.Core.Application.Dtos;

namespace Application.Features.BagMovements.Queries.GetList;

public class GetListBagMovementListItemDto : IDto
{
    public Guid Id { get; set; }
    public Guid BagId { get; set; }
    public Guid? FromSlotId { get; set; }
    public Guid? ToSlotId { get; set; }
    public string Action { get; set; }
    public DateTime CreatedDate { get; set; }
}