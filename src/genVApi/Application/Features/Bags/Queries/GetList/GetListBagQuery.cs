using Application.Features.Bags.Constants;
using Application.Services.Repositories;
using AutoMapper;
using Domain.Entities;
using NArchitecture.Core.Application.Pipelines.Authorization;
using NArchitecture.Core.Application.Pipelines.Caching;
using NArchitecture.Core.Application.Requests;
using NArchitecture.Core.Application.Responses;
using NArchitecture.Core.Persistence.Paging;
using MediatR;
using static Application.Features.Bags.Constants.BagsOperationClaims;

namespace Application.Features.Bags.Queries.GetList;

public class GetListBagQuery : IRequest<GetListResponse<GetListBagListItemDto>>, ISecuredRequest, ICachableRequest
{
    public PageRequest PageRequest { get; set; }

    public string[] Roles => [Admin, Read];

    public bool BypassCache { get; }
    public string? CacheKey => $"GetListBags({PageRequest.PageIndex},{PageRequest.PageSize})";
    public string? CacheGroupKey => "GetBags";
    public TimeSpan? SlidingExpiration { get; }

    public class GetListBagQueryHandler : IRequestHandler<GetListBagQuery, GetListResponse<GetListBagListItemDto>>
    {
        private readonly IBagRepository _bagRepository;
        private readonly IMapper _mapper;

        public GetListBagQueryHandler(IBagRepository bagRepository, IMapper mapper)
        {
            _bagRepository = bagRepository;
            _mapper = mapper;
        }

        public async Task<GetListResponse<GetListBagListItemDto>> Handle(GetListBagQuery request, CancellationToken cancellationToken)
        {
            IPaginate<Bag> bags = await _bagRepository.GetListAsync(
                index: request.PageRequest.PageIndex,
                size: request.PageRequest.PageSize, 
                cancellationToken: cancellationToken
            );

            GetListResponse<GetListBagListItemDto> response = _mapper.Map<GetListResponse<GetListBagListItemDto>>(bags);
            return response;
        }
    }
}