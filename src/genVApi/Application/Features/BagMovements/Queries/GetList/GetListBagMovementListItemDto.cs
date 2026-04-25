using NArchitecture.Core.Application.Dtos;

namespace Application.Features.BagMovements.Queries.GetList;

public class GetListBagMovementListItemDto : IDto
{
    public Guid Id { get; set; }
    public Guid BagId { get; set; }
    public Guid? FromBagCellId { get; set; }
    public Guid? ToBagCellId { get; set; }
    public string Action { get; set; }
    public DateTime CreatedDate { get; set; }
}