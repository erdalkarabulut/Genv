using Application.Features.CollectionSessions.Constants;
using Application.Features.CollectionSessions.Queries.GetList;
using Application.Services.Repositories;
using AutoMapper;
using Domain.Entities;
using NArchitecture.Core.Application.Pipelines.Authorization;
using MediatR;
using static Application.Features.CollectionSessions.Constants.CollectionSessionsOperationClaims;
using NArchitecture.Core.Application.Requests;
using NArchitecture.Core.Application.Responses;
using NArchitecture.Core.Persistence.Dynamic;
using NArchitecture.Core.Persistence.Paging;

namespace Application.Features.CollectionSessions.Queries.GetListByDynamic;

public class GetListByDynamicCollectionSessionQuery : IRequest<GetListResponse<GetListCollectionSessionListItemDto>>, ISecuredRequest
{
    public PageRequest PageRequest { get; set; }
    public DynamicQuery? Dynamic { get; set; }

    public string[] Roles => [Admin, Read];

    public class GetListByDynamicCollectionSessionQueryHandler : IRequestHandler<GetListByDynamicCollectionSessionQuery, GetListResponse<GetListCollectionSessionListItemDto>>
    {
        private readonly ICollectionSessionRepository _collectionSessionRepository;
        private readonly IMapper _mapper;

        public GetListByDynamicCollectionSessionQueryHandler(ICollectionSessionRepository collectionSessionRepository, IMapper mapper)
        {
            _collectionSessionRepository = collectionSessionRepository;
            _mapper = mapper;
        }

        public async Task<GetListResponse<GetListCollectionSessionListItemDto>> Handle(GetListByDynamicCollectionSessionQuery request, CancellationToken cancellationToken)
        {
            DynamicQuery dynamicQuery = request.Dynamic ?? new DynamicQuery();

            IPaginate<CollectionSession> collectionSessions = await _collectionSessionRepository.GetListByDynamicAsync(
                dynamicQuery,
                index: request.PageRequest.PageIndex,
                size: request.PageRequest.PageSize,
                cancellationToken: cancellationToken,
                enableTracking: false);

            GetListResponse<GetListCollectionSessionListItemDto> response = _mapper.Map<GetListResponse<GetListCollectionSessionListItemDto>>(collectionSessions);
            return response;
        }
    }
}
