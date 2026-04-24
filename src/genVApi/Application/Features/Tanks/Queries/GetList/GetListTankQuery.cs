using Application.Features.Tanks.Constants;
using Application.Services.Repositories;
using AutoMapper;
using Domain.Entities;
using NArchitecture.Core.Application.Pipelines.Authorization;
using NArchitecture.Core.Application.Pipelines.Caching;
using NArchitecture.Core.Application.Requests;
using NArchitecture.Core.Application.Responses;
using NArchitecture.Core.Persistence.Paging;
using MediatR;
using static Application.Features.Tanks.Constants.TanksOperationClaims;

namespace Application.Features.Tanks.Queries.GetList;

public class GetListTankQuery : IRequest<GetListResponse<GetListTankListItemDto>>, ISecuredRequest, ICachableRequest
{
    public PageRequest PageRequest { get; set; }

    public string[] Roles => [Admin, Read];

    public bool BypassCache { get; }
    public string? CacheKey => $"GetListTanks({PageRequest.PageIndex},{PageRequest.PageSize})";
    public string? CacheGroupKey => "GetTanks";
    public TimeSpan? SlidingExpiration { get; }

    public class GetListTankQueryHandler : IRequestHandler<GetListTankQuery, GetListResponse<GetListTankListItemDto>>
    {
        private readonly ITankRepository _tankRepository;
        private readonly IMapper _mapper;

        public GetListTankQueryHandler(ITankRepository tankRepository, IMapper mapper)
        {
            _tankRepository = tankRepository;
            _mapper = mapper;
        }

        public async Task<GetListResponse<GetListTankListItemDto>> Handle(GetListTankQuery request, CancellationToken cancellationToken)
        {
            IPaginate<Tank> tanks = await _tankRepository.GetListAsync(
                index: request.PageRequest.PageIndex,
                size: request.PageRequest.PageSize, 
                cancellationToken: cancellationToken
            );

            GetListResponse<GetListTankListItemDto> response = _mapper.Map<GetListResponse<GetListTankListItemDto>>(tanks);
            return response;
        }
    }
}