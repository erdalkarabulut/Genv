using NArchitecture.Core.Application.Responses;

namespace Application.Features.Bags.Commands.CreateRange;

public class CreatedBagRangeResponse : IResponse
{
    public ICollection<Guid> Ids { get; set; } = new List<Guid>();
}
