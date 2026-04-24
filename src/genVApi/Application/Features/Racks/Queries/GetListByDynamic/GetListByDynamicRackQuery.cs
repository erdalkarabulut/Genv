using Application.Features.Racks.Constants;
using Application.Features.Racks.Queries.GetList;
using Application.Services.Repositories;
using AutoMapper;
using Domain.Entities;
using NArchitecture.Core.Application.Pipelines.Authorization;
using MediatR;
using static Application.Features.Racks.Constants.RacksOperationClaims;
using NArchitecture.Core.Application.Requests;
using NArchitecture.Core.Application.Responses;
using NArchitecture.Core.Persistence.Dynamic;
using NArchitecture.Core.Persistence.Paging;

namespace Application.Features.Racks.Queries.GetListByDynamic;

public class GetListByDynamicRackQuery : IRequest<GetListResponse<GetListRackListItemDto>>, ISecuredRequest
{
    public PageRequest PageRequest { get; set; }
    public DynamicQuery? Dynamic { get; set; }

    public string[] Roles => [Admin, Read];

    public class GetListByDynamicRackQueryHandler : IRequestHandler<GetListByDynamicRackQuery, GetListResponse<GetListRackListItemDto>>
    {
        private readonly IRackRepository _rackRepository;
        private readonly IMapper _mapper;

        public GetListByDynamicRackQueryHandler(IRackRepository rackRepository, IMapper mapper)
        {
            _rackRepository = rackRepository;
            _mapper = mapper;
        }

        public async Task<GetListResponse<GetListRackListItemDto>> Handle(GetListByDynamicRackQuery request, CancellationToken cancellationToken)
        {
            DynamicQuery dynamicQuery = request.Dynamic ?? new DynamicQuery();

            IPaginate<Rack> racks = await _rackRepository.GetListByDynamicAsync(
                dynamicQuery,
                index: request.PageRequest.PageIndex,
                size: request.PageRequest.PageSize,
                cancellationToken: cancellationToken,
                enableTracking: false);

            GetListResponse<GetListRackListItemDto> response = _mapper.Map<GetListResponse<GetListRackListItemDto>>(racks);
            return response;
        }
    }
}
