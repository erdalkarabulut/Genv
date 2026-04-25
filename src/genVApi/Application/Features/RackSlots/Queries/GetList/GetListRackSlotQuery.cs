using Application.Services.Repositories;
using AutoMapper;
using Domain.Entities;
using MediatR;
using NArchitecture.Core.Application.Pipelines.Authorization;
using NArchitecture.Core.Application.Pipelines.Caching;
using NArchitecture.Core.Application.Requests;
using NArchitecture.Core.Application.Responses;
using NArchitecture.Core.Persistence.Paging;
using static Application.Features.Racks.Constants.RacksOperationClaims;

namespace Application.Features.RackSlots.Queries.GetList;

public class GetListRackSlotQuery : IRequest<GetListResponse<GetListRackSlotListItemDto>>, ISecuredRequest, ICachableRequest
{
    public PageRequest PageRequest { get; set; } = default!;
    public Guid? RackId { get; set; }

    public string[] Roles => [Admin, Read];

    public bool BypassCache { get; }
    public string? CacheKey =>
        $"GetListRackSlots({PageRequest.PageIndex},{PageRequest.PageSize},{RackId})";

    public string? CacheGroupKey => "GetRackSlots";
    public TimeSpan? SlidingExpiration { get; }

    public class Handler : IRequestHandler<GetListRackSlotQuery, GetListResponse<GetListRackSlotListItemDto>>
    {
        private readonly ISlotRepository _rackSlotRepository;
        private readonly IMapper _mapper;

        public Handler(ISlotRepository rackSlotRepository, IMapper mapper)
        {
            _rackSlotRepository = rackSlotRepository;
            _mapper = mapper;
        }

        public async Task<GetListResponse<GetListRackSlotListItemDto>> Handle(
            GetListRackSlotQuery request,
            CancellationToken cancellationToken)
        {
            IPaginate<Slot> slots = await _rackSlotRepository.GetListAsync(
                predicate: request.RackId.HasValue ? s => s.RackId == request.RackId.Value : null,
                index: request.PageRequest.PageIndex,
                size: request.PageRequest.PageSize,
                cancellationToken: cancellationToken
            );

            return _mapper.Map<GetListResponse<GetListRackSlotListItemDto>>(slots);
        }
    }
}
