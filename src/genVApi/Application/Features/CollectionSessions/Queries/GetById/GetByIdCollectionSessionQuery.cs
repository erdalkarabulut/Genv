using Application.Features.CollectionSessions.Constants;
using Application.Features.CollectionSessions.Rules;
using Application.Services.Repositories;
using AutoMapper;
using Domain.Entities;
using NArchitecture.Core.Application.Pipelines.Authorization;
using MediatR;
using static Application.Features.CollectionSessions.Constants.CollectionSessionsOperationClaims;

namespace Application.Features.CollectionSessions.Queries.GetById;

public class GetByIdCollectionSessionQuery : IRequest<GetByIdCollectionSessionResponse>, ISecuredRequest
{
    public Guid Id { get; set; }

    public string[] Roles => [Admin, Read];

    public class GetByIdCollectionSessionQueryHandler : IRequestHandler<GetByIdCollectionSessionQuery, GetByIdCollectionSessionResponse>
    {
        private readonly IMapper _mapper;
        private readonly ICollectionSessionRepository _collectionSessionRepository;
        private readonly CollectionSessionBusinessRules _collectionSessionBusinessRules;

        public GetByIdCollectionSessionQueryHandler(IMapper mapper, ICollectionSessionRepository collectionSessionRepository, CollectionSessionBusinessRules collectionSessionBusinessRules)
        {
            _mapper = mapper;
            _collectionSessionRepository = collectionSessionRepository;
            _collectionSessionBusinessRules = collectionSessionBusinessRules;
        }

        public async Task<GetByIdCollectionSessionResponse> Handle(GetByIdCollectionSessionQuery request, CancellationToken cancellationToken)
        {
            CollectionSession? collectionSession = await _collectionSessionRepository.GetAsync(predicate: cs => cs.Id == request.Id, cancellationToken: cancellationToken);
            await _collectionSessionBusinessRules.CollectionSessionShouldExistWhenSelected(collectionSession);

            GetByIdCollectionSessionResponse response = _mapper.Map<GetByIdCollectionSessionResponse>(collectionSession);
            return response;
        }
    }
}