using Application.Features.Bags.Constants;
using Application.Features.Bags.Queries.GetList;
using Application.Services.Repositories;
using AutoMapper;
using Domain.Entities;
using NArchitecture.Core.Application.Pipelines.Authorization;
using MediatR;
using static Application.Features.Bags.Constants.BagsOperationClaims;
using NArchitecture.Core.Application.Requests;
using NArchitecture.Core.Application.Responses;
using NArchitecture.Core.Persistence.Dynamic;
using NArchitecture.Core.Persistence.Paging;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Bags.Queries.GetListByDynamic;

public class GetListByDynamicBagQuery : IRequest<GetListResponse<GetListBagListItemDto>>, ISecuredRequest
{
    public PageRequest PageRequest { get; set; }
    public DynamicQuery? Dynamic { get; set; }

    public string[] Roles => [Admin, Read];

    public class GetListByDynamicBagQueryHandler : IRequestHandler<GetListByDynamicBagQuery, GetListResponse<GetListBagListItemDto>>
    {
        private readonly IBagRepository _bagRepository;
        private readonly IMapper _mapper;

        public GetListByDynamicBagQueryHandler(IBagRepository bagRepository, IMapper mapper)
        {
            _bagRepository = bagRepository;
            _mapper = mapper;
        }

        public async Task<GetListResponse<GetListBagListItemDto>> Handle(GetListByDynamicBagQuery request, CancellationToken cancellationToken)
        {
            DynamicQuery dynamicQuery = request.Dynamic ?? new DynamicQuery();

            IPaginate<Bag> bags = await _bagRepository.GetListByDynamicAsync(
                dynamicQuery,
                include: m => m.Include(b => b.BagCell).ThenInclude(c => c!.Box).ThenInclude(b => b.Slot).ThenInclude(s => s.Rack).ThenInclude(r => r.Tank),
                index: request.PageRequest.PageIndex,
                size: request.PageRequest.PageSize,
                cancellationToken: cancellationToken,
                enableTracking: false);

            GetListResponse<GetListBagListItemDto> response = _mapper.Map<GetListResponse<GetListBagListItemDto>>(bags);
            return response;
        }
    }
}
