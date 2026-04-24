using NArchitecture.Core.Application.Responses;

namespace Application.Features.BagMovements.Commands.CreateRange;

public class CreatedBagMovementRangeResponse : IResponse
{
    public ICollection<Guid> Ids { get; set; } = new List<Guid>();
}
