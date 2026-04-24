using NArchitecture.Core.Application.Responses;

namespace Application.Features.Bags.Commands.UpdateRange;

public class UpdatedBagRangeResponse : IResponse
{
    public ICollection<Guid> Ids { get; set; } = new List<Guid>();
}
