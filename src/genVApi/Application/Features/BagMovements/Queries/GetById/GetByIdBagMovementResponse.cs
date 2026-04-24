using NArchitecture.Core.Application.Responses;

namespace Application.Features.BagMovements.Queries.GetById;

public class GetByIdBagMovementResponse : IResponse
{
    public Guid Id { get; set; }
    public Guid BagId { get; set; }
    public Guid? FromSlotId { get; set; }
    public Guid? ToSlotId { get; set; }
    public string Action { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime? UpdatedDate { get; set; }
}