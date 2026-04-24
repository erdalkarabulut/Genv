using NArchitecture.Core.Application.Responses;

namespace Application.Features.Racks.Commands.CreateRange;

public class CreatedRackRangeResponse : IResponse
{
    public ICollection<Guid> Ids { get; set; } = new List<Guid>();
}
