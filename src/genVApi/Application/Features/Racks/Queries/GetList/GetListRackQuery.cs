using Application.Features.Racks.Constants;
using Application.Services.Repositories;
using AutoMapper;
using Domain.Entities;
using NArchitecture.Core.Application.Pipelines.Authorization;
using NArchitecture.Core.Application.Pipelines.Caching;
using NArchitecture.Core.Application.Requests;
using NArchitecture.Core.Application.Responses;
using NArchitecture.Core.Persistence.Paging;
using MediatR;
using static Application.Features.Racks.Constants.RacksOperationClaims;

namespace Application.Features.Racks.Queries.GetList;

public class GetListRackQuery : IRequest<GetListResponse<GetListRackListItemDto>>, ISecuredRequest, ICachableRequest
{
    public PageRequest PageRequest { get; set; }

    public string[] Roles => [Admin, Read];

    public bool BypassCache { get; }
    public string? CacheKey => $"GetListRacks({PageRequest.PageIndex},{PageRequest.PageSize})";
    public string? CacheGroupKey => "GetRacks";
    public TimeSpan? SlidingExpiration { get; }

    public class GetListRackQueryHandler : IRequestHandler<GetListRackQuery, GetListResponse<GetListRackListItemDto>>
    {
        private readonly IRackRepository _rackRepository;
        private readonly IMapper _mapper;

        public GetListRackQueryHandler(IRackRepository rackRepository, IMapper mapper)
        {
            _rackRepository = rackRepository;
            _mapper = mapper;
        }

        public async Task<GetListResponse<GetListRackListItemDto>> Handle(GetListRackQuery request, CancellationToken cancellationToken)
        {
            IPaginate<Rack> racks = await _rackRepository.GetListAsync(
                index: request.PageRequest.PageIndex,
                size: request.PageRequest.PageSize, 
                cancellationToken: cancellationToken
            );

            GetListResponse<GetListRackListItemDto> response = _mapper.Map<GetListResponse<GetListRackListItemDto>>(racks);
            return response;
        }
    }
}