using NArchitecture.Core.Application.Responses;

namespace Application.Features.BagMovements.Commands.UpdateRange;

public class UpdatedBagMovementRangeResponse : IResponse
{
    public ICollection<Guid> Ids { get; set; } = new List<Guid>();
}
