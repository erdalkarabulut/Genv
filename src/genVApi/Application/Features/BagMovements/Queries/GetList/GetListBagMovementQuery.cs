using Application.Features.BagMovements.Constants;
using Application.Services.Repositories;
using AutoMapper;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using NArchitecture.Core.Application.Pipelines.Authorization;
using NArchitecture.Core.Application.Pipelines.Caching;
using NArchitecture.Core.Application.Requests;
using NArchitecture.Core.Application.Responses;
using NArchitecture.Core.Persistence.Paging;
using MediatR;
using static Application.Features.BagMovements.Constants.BagMovementsOperationClaims;

namespace Application.Features.BagMovements.Queries.GetList;

public class GetListBagMovementQuery : IRequest<GetListResponse<GetListBagMovementListItemDto>>, ISecuredRequest, ICachableRequest
{
    public PageRequest PageRequest { get; set; }

    public string[] Roles => [Admin, Read];

    public bool BypassCache { get; }
    public string? CacheKey => $"GetListBagMovements({PageRequest.PageIndex},{PageRequest.PageSize})";
    public string? CacheGroupKey => "GetBagMovements";
    public TimeSpan? SlidingExpiration { get; }

    public class GetListBagMovementQueryHandler : IRequestHandler<GetListBagMovementQuery, GetListResponse<GetListBagMovementListItemDto>>
    {
        private readonly IBagMovementRepository _bagMovementRepository;
        private readonly IMapper _mapper;

        public GetListBagMovementQueryHandler(IBagMovementRepository bagMovementRepository, IMapper mapper)
        {
            _bagMovementRepository = bagMovementRepository;
            _mapper = mapper;
        }

        public async Task<GetListResponse<GetListBagMovementListItemDto>> Handle(GetListBagMovementQuery request, CancellationToken cancellationToken)
        {
            IPaginate<BagMovement> bagMovements = await _bagMovementRepository.GetListAsync(
                include: m => m.Include(bm => bm.Bag).ThenInclude(b => b.Session).ThenInclude(s => s.Patient)
                           .Include(bm => bm.FromBagCell).ThenInclude(fbc => fbc!.Box).ThenInclude(b => b.Slot).ThenInclude(s => s.Rack).ThenInclude(r => r.Tank)
                           .Include(bm => bm.ToBagCell).ThenInclude(tbc => tbc!.Box).ThenInclude(b => b.Slot).ThenInclude(s => s.Rack).ThenInclude(r => r.Tank),
                index: request.PageRequest.PageIndex,
                size: request.PageRequest.PageSize,
                cancellationToken: cancellationToken,
                enableTracking: false
            );

            GetListResponse<GetListBagMovementListItemDto> response = _mapper.Map<GetListResponse<GetListBagMovementListItemDto>>(bagMovements);
            return response;
        }
    }
}