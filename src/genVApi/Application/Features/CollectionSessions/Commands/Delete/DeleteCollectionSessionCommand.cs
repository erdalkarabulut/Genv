using Application.Features.CollectionSessions.Constants;
using Application.Features.CollectionSessions.Constants;
using Application.Features.CollectionSessions.Rules;
using Application.Services.Repositories;
using AutoMapper;
using Domain.Entities;
using NArchitecture.Core.Application.Pipelines.Authorization;
using NArchitecture.Core.Application.Pipelines.Caching;
using NArchitecture.Core.Application.Pipelines.Logging;
using NArchitecture.Core.Application.Pipelines.Transaction;
using NArchitecture.Core.CrossCuttingConcerns.Exception.Types;
using MediatR;
using static Application.Features.CollectionSessions.Constants.CollectionSessionsOperationClaims;

namespace Application.Features.CollectionSessions.Commands.Delete;

public class DeleteCollectionSessionCommand : IRequest<DeletedCollectionSessionResponse>, ISecuredRequest, ICacheRemoverRequest, ILoggableRequest, ITransactionalRequest
{
    public Guid Id { get; set; }

    public string[] Roles => [Admin, Write, CollectionSessionsOperationClaims.Delete];

    public bool BypassCache { get; }
    public string? CacheKey { get; }
    public string[]? CacheGroupKey => ["GetCollectionSessions", "GetPatients", "Dashboard"];

    public class DeleteCollectionSessionCommandHandler : IRequestHandler<DeleteCollectionSessionCommand, DeletedCollectionSessionResponse>
    {
        private readonly IMapper _mapper;
        private readonly ICollectionSessionRepository _collectionSessionRepository;
        private readonly IBagRepository _bagRepository;
        private readonly CollectionSessionBusinessRules _collectionSessionBusinessRules;

        public DeleteCollectionSessionCommandHandler(IMapper mapper, ICollectionSessionRepository collectionSessionRepository,
                                         IBagRepository bagRepository,
                                         CollectionSessionBusinessRules collectionSessionBusinessRules)
        {
            _mapper = mapper;
            _collectionSessionRepository = collectionSessionRepository;
            _bagRepository = bagRepository;
            _collectionSessionBusinessRules = collectionSessionBusinessRules;
        }

        public async Task<DeletedCollectionSessionResponse> Handle(DeleteCollectionSessionCommand request, CancellationToken cancellationToken)
        {
            CollectionSession? collectionSession = await _collectionSessionRepository.GetAsync(predicate: cs => cs.Id == request.Id, cancellationToken: cancellationToken);
            await _collectionSessionBusinessRules.CollectionSessionShouldExistWhenSelected(collectionSession);

            bool hasBags = await _bagRepository.AnyAsync(
                predicate: b => b.SessionId == request.Id,
                enableTracking: false,
                cancellationToken: cancellationToken);
            if (hasBags)
                throw new BusinessException("Bu seansa bağlı torbalar var. Önce torbaları silin.");

            await _collectionSessionRepository.DeleteAsync(collectionSession!, permanent: true);

            DeletedCollectionSessionResponse response = _mapper.Map<DeletedCollectionSessionResponse>(collectionSession);
            return response;
        }
    }
}