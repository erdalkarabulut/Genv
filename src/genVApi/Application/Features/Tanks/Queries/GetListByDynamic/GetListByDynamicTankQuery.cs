using Application.Features.Tanks.Constants;
using Application.Features.Tanks.Queries.GetList;
using Application.Services.Repositories;
using AutoMapper;
using Domain.Entities;
using NArchitecture.Core.Application.Pipelines.Authorization;
using MediatR;
using static Application.Features.Tanks.Constants.TanksOperationClaims;
using NArchitecture.Core.Application.Requests;
using NArchitecture.Core.Application.Responses;
using NArchitecture.Core.Persistence.Dynamic;
using NArchitecture.Core.Persistence.Paging;

namespace Application.Features.Tanks.Queries.GetListByDynamic;

public class GetListByDynamicTankQuery : IRequest<GetListResponse<GetListTankListItemDto>>, ISecuredRequest
{
    public PageRequest PageRequest { get; set; }
    public DynamicQuery? Dynamic { get; set; }

    public string[] Roles => [Admin, Read];

    public class GetListByDynamicTankQueryHandler : IRequestHandler<GetListByDynamicTankQuery, GetListResponse<GetListTankListItemDto>>
    {
        private readonly ITankRepository _tankRepository;
        private readonly IMapper _mapper;

        public GetListByDynamicTankQueryHandler(ITankRepository tankRepository, IMapper mapper)
        {
            _tankRepository = tankRepository;
            _mapper = mapper;
        }

        public async Task<GetListResponse<GetListTankListItemDto>> Handle(GetListByDynamicTankQuery request, CancellationToken cancellationToken)
        {
            DynamicQuery dynamicQuery = request.Dynamic ?? new DynamicQuery();

            IPaginate<Tank> tanks = await _tankRepository.GetListByDynamicAsync(
                dynamicQuery,
                index: request.PageRequest.PageIndex,
                size: request.PageRequest.PageSize,
                cancellationToken: cancellationToken,
                enableTracking: false);

            GetListResponse<GetListTankListItemDto> response = _mapper.Map<GetListResponse<GetListTankListItemDto>>(tanks);
            return response;
        }
    }
}
