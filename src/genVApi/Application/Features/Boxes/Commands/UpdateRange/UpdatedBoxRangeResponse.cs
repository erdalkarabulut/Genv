using NArchitecture.Core.Application.Responses;

namespace Application.Features.Boxes.Commands.UpdateRange;

public class UpdatedBoxRangeResponse : IResponse
{
    public ICollection<Guid> Ids { get; set; } = new List<Guid>();
}
