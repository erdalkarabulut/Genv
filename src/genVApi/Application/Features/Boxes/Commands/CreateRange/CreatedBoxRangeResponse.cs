using NArchitecture.Core.Application.Responses;

namespace Application.Features.Boxes.Commands.CreateRange;

public class CreatedBoxRangeResponse : IResponse
{
    public ICollection<Guid> Ids { get; set; } = new List<Guid>();
}
