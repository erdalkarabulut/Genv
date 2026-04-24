using Application.Features.Slots.Constants;
using Application.Features.Slots.Queries.GetList;
using Application.Services.Repositories;
using AutoMapper;
using Domain.Entities;
using NArchitecture.Core.Application.Pipelines.Authorization;
using MediatR;
using static Application.Features.Slots.Constants.SlotsOperationClaims;
using NArchitecture.Core.Application.Requests;
using NArchitecture.Core.Application.Responses;
using NArchitecture.Core.Persistence.Dynamic;
using NArchitecture.Core.Persistence.Paging;

namespace Application.Features.Slots.Queries.GetListByDynamic;

public class GetListByDynamicSlotQuery : IRequest<GetListResponse<GetListSlotListItemDto>>, ISecuredRequest
{
    public PageRequest PageRequest { get; set; }
    public DynamicQuery? Dynamic { get; set; }

    public string[] Roles => [Admin, Read];

    public class GetListByDynamicSlotQueryHandler : IRequestHandler<GetListByDynamicSlotQuery, GetListResponse<GetListSlotListItemDto>>
    {
        private readonly ISlotRepository _slotRepository;
        private readonly IMapper _mapper;

        public GetListByDynamicSlotQueryHandler(ISlotRepository slotRepository, IMapper mapper)
        {
            _slotRepository = slotRepository;
            _mapper = mapper;
        }

        public async Task<GetListResponse<GetListSlotListItemDto>> Handle(GetListByDynamicSlotQuery request, CancellationToken cancellationToken)
        {
            DynamicQuery dynamicQuery = request.Dynamic ?? new DynamicQuery();

            IPaginate<Slot> slots = await _slotRepository.GetListByDynamicAsync(
                dynamicQuery,
                index: request.PageRequest.PageIndex,
                size: request.PageRequest.PageSize,
                cancellationToken: cancellationToken,
                enableTracking: false);

            GetListResponse<GetListSlotListItemDto> response = _mapper.Map<GetListResponse<GetListSlotListItemDto>>(slots);
            return response;
        }
    }
}
