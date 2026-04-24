using NArchitecture.Core.Application.Responses;

namespace Application.Features.CollectionSessions.Commands.Delete;

public class DeletedCollectionSessionResponse : IResponse
{
    public Guid Id { get; set; }
}