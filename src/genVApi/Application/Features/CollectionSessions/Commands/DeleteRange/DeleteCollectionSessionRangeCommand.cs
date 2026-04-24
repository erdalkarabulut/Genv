using Application.Features.CollectionSessions.Constants;
using Application.Features.CollectionSessions.Rules;
using Application.Services.Repositories;
using Domain.Entities;
using NArchitecture.Core.Application.Pipelines.Authorization;
using NArchitecture.Core.Application.Pipelines.Caching;
using NArchitecture.Core.Application.Pipelines.Logging;
using NArchitecture.Core.Application.Pipelines.Transaction;
using MediatR;
using static Application.Features.CollectionSessions.Constants.CollectionSessionsOperationClaims;

namespace Application.Features.CollectionSessions.Commands.DeleteRange;

public class DeleteCollectionSessionRangeCommand : IRequest<DeletedCollectionSessionRangeResponse>, ISecuredRequest, ICacheRemoverRequest, ILoggableRequest, ITransactionalRequest
{
    public ICollection<Guid> Ids { get; set; } = new List<Guid>();

    public string[] Roles => [Admin, Write, CollectionSessionsOperationClaims.DeleteRange];

    public bool BypassCache { get; }
    public string? CacheKey { get; }
    public string[]? CacheGroupKey => ["GetCollectionSessions"];

    public class DeleteCollectionSessionRangeCommandHandler : IRequestHandler<DeleteCollectionSessionRangeCommand, DeletedCollectionSessionRangeResponse>
    {
        private readonly ICollectionSessionRepository _collectionSessionRepository;
        private readonly CollectionSessionBusinessRules _collectionSessionBusinessRules;

        public DeleteCollectionSessionRangeCommandHandler(ICollectionSessionRepository collectionSessionRepository, CollectionSessionBusinessRules collectionSessionBusinessRules)
        {
            _collectionSessionRepository = collectionSessionRepository;
            _collectionSessionBusinessRules = collectionSessionBusinessRules;
        }

        public async Task<DeletedCollectionSessionRangeResponse> Handle(DeleteCollectionSessionRangeCommand request, CancellationToken cancellationToken)
        {
            List<CollectionSession> collectionSessions = new List<CollectionSession>();

            foreach (Guid id in request.Ids)
            {
                CollectionSession? collectionSession = await _collectionSessionRepository.GetAsync(
                    predicate: cs => cs.Id == id,
                    cancellationToken: cancellationToken
                );
                await _collectionSessionBusinessRules.CollectionSessionShouldExistWhenSelected(collectionSession);
                collectionSessions.Add(collectionSession!);
            }

            await _collectionSessionRepository.DeleteRangeAsync(collectionSessions);

            return new DeletedCollectionSessionRangeResponse { DeletedCount = request.Ids.Count };
        }
    }
}
