using NArchitecture.Core.Application.Responses;

namespace Application.Features.BagMovements.Commands.Create;

public class CreatedBagMovementResponse : IResponse
{
    public Guid Id { get; set; }
    public Guid BagId { get; set; }
    public Guid? FromSlotId { get; set; }
    public Guid? ToSlotId { get; set; }
    public string Action { get; set; }
}