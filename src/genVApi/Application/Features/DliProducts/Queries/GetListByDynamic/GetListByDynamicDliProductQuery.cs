using Application.Features.DliProducts.Constants;
using Application.Features.DliProducts.Queries.GetList;
using Application.Services.Repositories;
using AutoMapper;
using Domain.Entities;
using NArchitecture.Core.Application.Pipelines.Authorization;
using MediatR;
using static Application.Features.DliProducts.Constants.DliProductsOperationClaims;
using NArchitecture.Core.Application.Requests;
using NArchitecture.Core.Application.Responses;
using NArchitecture.Core.Persistence.Dynamic;
using NArchitecture.Core.Persistence.Paging;

namespace Application.Features.DliProducts.Queries.GetListByDynamic;

public class GetListByDynamicDliProductQuery : IRequest<GetListResponse<GetListDliProductListItemDto>>, ISecuredRequest
{
    public PageRequest PageRequest { get; set; }
    public DynamicQuery? Dynamic { get; set; }

    public string[] Roles => [Admin, Read];

    public class GetListByDynamicDliProductQueryHandler : IRequestHandler<GetListByDynamicDliProductQuery, GetListResponse<GetListDliProductListItemDto>>
    {
        private readonly IDliProductRepository _dliProductRepository;
        private readonly IMapper _mapper;

        public GetListByDynamicDliProductQueryHandler(IDliProductRepository dliProductRepository, IMapper mapper)
        {
            _dliProductRepository = dliProductRepository;
            _mapper = mapper;
        }

        public async Task<GetListResponse<GetListDliProductListItemDto>> Handle(GetListByDynamicDliProductQuery request, CancellationToken cancellationToken)
        {
            DynamicQuery dynamicQuery = request.Dynamic ?? new DynamicQuery();

            IPaginate<DliProduct> dliProducts = await _dliProductRepository.GetListByDynamicAsync(
                dynamicQuery,
                index: request.PageRequest.PageIndex,
                size: request.PageRequest.PageSize,
                cancellationToken: cancellationToken,
                enableTracking: false);

            GetListResponse<GetListDliProductListItemDto> response = _mapper.Map<GetListResponse<GetListDliProductListItemDto>>(dliProducts);
            return response;
        }
    }
}
