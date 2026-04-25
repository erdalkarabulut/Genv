using NArchitecture.Core.Application.Responses;

namespace Application.Features.BagMovements.Commands.Create;

public class CreatedBagMovementResponse : IResponse
{
    public Guid Id { get; set; }
    public Guid BagId { get; set; }
    public Guid? FromBagCellId { get; set; }
    public Guid? ToBagCellId { get; set; }
    public string Action { get; set; }
}