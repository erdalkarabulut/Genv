using Application.Features.BagMovements.Constants;
using Application.Features.BagMovements.Queries.GetList;
using Application.Services.Repositories;
using AutoMapper;
using Domain.Entities;
using NArchitecture.Core.Application.Pipelines.Authorization;
using MediatR;
using static Application.Features.BagMovements.Constants.BagMovementsOperationClaims;
using NArchitecture.Core.Application.Requests;
using NArchitecture.Core.Application.Responses;
using NArchitecture.Core.Persistence.Dynamic;
using NArchitecture.Core.Persistence.Paging;

namespace Application.Features.BagMovements.Queries.GetListByDynamic;

public class GetListByDynamicBagMovementQuery : IRequest<GetListResponse<GetListBagMovementListItemDto>>, ISecuredRequest
{
    public PageRequest PageRequest { get; set; }
    public DynamicQuery? Dynamic { get; set; }

    public string[] Roles => [Admin, Read];

    public class GetListByDynamicBagMovementQueryHandler : IRequestHandler<GetListByDynamicBagMovementQuery, GetListResponse<GetListBagMovementListItemDto>>
    {
        private readonly IBagMovementRepository _bagMovementRepository;
        private readonly IMapper _mapper;

        public GetListByDynamicBagMovementQueryHandler(IBagMovementRepository bagMovementRepository, IMapper mapper)
        {
            _bagMovementRepository = bagMovementRepository;
            _mapper = mapper;
        }

        public async Task<GetListResponse<GetListBagMovementListItemDto>> Handle(GetListByDynamicBagMovementQuery request, CancellationToken cancellationToken)
        {
            DynamicQuery dynamicQuery = request.Dynamic ?? new DynamicQuery();

            IPaginate<BagMovement> bagMovements = await _bagMovementRepository.GetListByDynamicAsync(
                dynamicQuery,
                index: request.PageRequest.PageIndex,
                size: request.PageRequest.PageSize,
                cancellationToken: cancellationToken,
                enableTracking: false);

            GetListResponse<GetListBagMovementListItemDto> response = _mapper.Map<GetListResponse<GetListBagMovementListItemDto>>(bagMovements);
            return response;
        }
    }
}
