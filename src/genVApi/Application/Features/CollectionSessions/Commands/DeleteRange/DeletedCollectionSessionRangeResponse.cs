using NArchitecture.Core.Application.Responses;

namespace Application.Features.CollectionSessions.Commands.DeleteRange;

public class DeletedCollectionSessionRangeResponse : IResponse
{
    public int DeletedCount { get; set; }
}
