using Application.Features.CollectionSessions.Constants;
using Application.Services.Repositories;
using AutoMapper;
using Domain.Entities;
using NArchitecture.Core.Application.Pipelines.Authorization;
using NArchitecture.Core.Application.Pipelines.Caching;
using NArchitecture.Core.Application.Requests;
using NArchitecture.Core.Application.Responses;
using NArchitecture.Core.Persistence.Paging;
using MediatR;
using static Application.Features.CollectionSessions.Constants.CollectionSessionsOperationClaims;

namespace Application.Features.CollectionSessions.Queries.GetList;

public class GetListCollectionSessionQuery : IRequest<GetListResponse<GetListCollectionSessionListItemDto>>, ISecuredRequest, ICachableRequest
{
    public PageRequest PageRequest { get; set; }

    public string[] Roles => [Admin, Read];

    public bool BypassCache { get; }
    public string? CacheKey => $"GetListCollectionSessions({PageRequest.PageIndex},{PageRequest.PageSize})";
    public string? CacheGroupKey => "GetCollectionSessions";
    public TimeSpan? SlidingExpiration { get; }

    public class GetListCollectionSessionQueryHandler : IRequestHandler<GetListCollectionSessionQuery, GetListResponse<GetListCollectionSessionListItemDto>>
    {
        private readonly ICollectionSessionRepository _collectionSessionRepository;
        private readonly IMapper _mapper;

        public GetListCollectionSessionQueryHandler(ICollectionSessionRepository collectionSessionRepository, IMapper mapper)
        {
            _collectionSessionRepository = collectionSessionRepository;
            _mapper = mapper;
        }

        public async Task<GetListResponse<GetListCollectionSessionListItemDto>> Handle(GetListCollectionSessionQuery request, CancellationToken cancellationToken)
        {
            IPaginate<CollectionSession> collectionSessions = await _collectionSessionRepository.GetListAsync(
                index: request.PageRequest.PageIndex,
                size: request.PageRequest.PageSize, 
                cancellationToken: cancellationToken
            );

            GetListResponse<GetListCollectionSessionListItemDto> response = _mapper.Map<GetListResponse<GetListCollectionSessionListItemDto>>(collectionSessions);
            return response;
        }
    }
}