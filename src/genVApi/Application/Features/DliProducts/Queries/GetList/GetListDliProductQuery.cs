using Application.Features.DliProducts.Constants;
using Application.Services.Repositories;
using AutoMapper;
using Domain.Entities;
using NArchitecture.Core.Application.Pipelines.Authorization;
using NArchitecture.Core.Application.Pipelines.Caching;
using NArchitecture.Core.Application.Requests;
using NArchitecture.Core.Application.Responses;
using NArchitecture.Core.Persistence.Paging;
using MediatR;
using static Application.Features.DliProducts.Constants.DliProductsOperationClaims;

namespace Application.Features.DliProducts.Queries.GetList;

public class GetListDliProductQuery : IRequest<GetListResponse<GetListDliProductListItemDto>>, ISecuredRequest, ICachableRequest
{
    public PageRequest PageRequest { get; set; }

    public string[] Roles => [Admin, Read];

    public bool BypassCache { get; }
    public string? CacheKey => $"GetListDliProducts({PageRequest.PageIndex},{PageRequest.PageSize})";
    public string? CacheGroupKey => "GetDliProducts";
    public TimeSpan? SlidingExpiration { get; }

    public class GetListDliProductQueryHandler : IRequestHandler<GetListDliProductQuery, GetListResponse<GetListDliProductListItemDto>>
    {
        private readonly IDliProductRepository _dliProductRepository;
        private readonly IMapper _mapper;

        public GetListDliProductQueryHandler(IDliProductRepository dliProductRepository, IMapper mapper)
        {
            _dliProductRepository = dliProductRepository;
            _mapper = mapper;
        }

        public async Task<GetListResponse<GetListDliProductListItemDto>> Handle(GetListDliProductQuery request, CancellationToken cancellationToken)
        {
            IPaginate<DliProduct> dliProducts = await _dliProductRepository.GetListAsync(
                index: request.PageRequest.PageIndex,
                size: request.PageRequest.PageSize, 
                cancellationToken: cancellationToken
            );

            GetListResponse<GetListDliProductListItemDto> response = _mapper.Map<GetListResponse<GetListDliProductListItemDto>>(dliProducts);
            return response;
        }
    }
}