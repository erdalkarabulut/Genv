using NArchitecture.Core.Application.Responses;

namespace Application.Features.CollectionSessions.Commands.CreateRange;

public class CreatedCollectionSessionRangeResponse : IResponse
{
    public ICollection<Guid> Ids { get; set; } = new List<Guid>();
}
