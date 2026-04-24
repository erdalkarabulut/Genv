using NArchitecture.Core.Application.Responses;

namespace Application.Features.Racks.Commands.UpdateRange;

public class UpdatedRackRangeResponse : IResponse
{
    public ICollection<Guid> Ids { get; set; } = new List<Guid>();
}
