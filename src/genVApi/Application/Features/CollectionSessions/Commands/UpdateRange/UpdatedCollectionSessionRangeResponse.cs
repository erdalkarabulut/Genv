using NArchitecture.Core.Application.Responses;

namespace Application.Features.CollectionSessions.Commands.UpdateRange;

public class UpdatedCollectionSessionRangeResponse : IResponse
{
    public ICollection<Guid> Ids { get; set; } = new List<Guid>();
}
