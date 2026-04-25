using Application.Features.Slots.Constants;
using Application.Services.Repositories;
using AutoMapper;
using Domain.Entities;
using NArchitecture.Core.Application.Pipelines.Authorization;
using NArchitecture.Core.Application.Pipelines.Caching;
using NArchitecture.Core.Application.Requests;
using NArchitecture.Core.Application.Responses;
using NArchitecture.Core.Persistence.Paging;
using MediatR;
using static Application.Features.Slots.Constants.SlotsOperationClaims;

namespace Application.Features.Slots.Queries.GetList;

public class GetListSlotQuery : IRequest<GetListResponse<GetListSlotListItemDto>>, ISecuredRequest, ICachableRequest
{
    public PageRequest PageRequest { get; set; }

    public string[] Roles => [Admin, Read];

    public bool BypassCache { get; }
    public string? CacheKey => $"GetListBagCells({PageRequest.PageIndex},{PageRequest.PageSize})";
    public string? CacheGroupKey => "GetBagCells";
    public TimeSpan? SlidingExpiration { get; }

    public class GetListSlotQueryHandler : IRequestHandler<GetListSlotQuery, GetListResponse<GetListSlotListItemDto>>
    {
        private readonly IBagCellRepository _bagCellRepository;
        private readonly IMapper _mapper;

        public GetListSlotQueryHandler(IBagCellRepository bagCellRepository, IMapper mapper)
        {
            _bagCellRepository = bagCellRepository;
            _mapper = mapper;
        }

        public async Task<GetListResponse<GetListSlotListItemDto>> Handle(GetListSlotQuery request, CancellationToken cancellationToken)
        {
            IPaginate<BagCell> slots = await _bagCellRepository.GetListAsync(
                index: request.PageRequest.PageIndex,
                size: request.PageRequest.PageSize, 
                cancellationToken: cancellationToken
            );

            GetListResponse<GetListSlotListItemDto> response = _mapper.Map<GetListResponse<GetListSlotListItemDto>>(slots);
            return response;
        }
    }
}