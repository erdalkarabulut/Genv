using NArchitecture.Core.Application.Responses;

namespace Application.Features.BagMovements.Commands.DeleteRange;

public class DeletedBagMovementRangeResponse : IResponse
{
    public int DeletedCount { get; set; }
}
