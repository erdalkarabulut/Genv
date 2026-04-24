using Application.Features.OperationClaims.Constants;
using Application.Features.OperationClaims.Queries.GetList;
using Application.Services.Repositories;
using AutoMapper;
using Domain.Entities;
using MediatR;
using NArchitecture.Core.Application.Pipelines.Authorization;
using NArchitecture.Core.Application.Requests;
using NArchitecture.Core.Application.Responses;
using NArchitecture.Core.Persistence.Dynamic;
using NArchitecture.Core.Persistence.Paging;

namespace Application.Features.OperationClaims.Queries.GetListByDynamic;

public class GetListByDynamicOperationClaimQuery : IRequest<GetListResponse<GetListOperationClaimListItemDto>>, ISecuredRequest
{
    public PageRequest PageRequest { get; set; } = null!;
    public DynamicQuery? Dynamic { get; set; }

    public string[] Roles => [OperationClaimsOperationClaims.Read];

    public class GetListByDynamicOperationClaimQueryHandler : IRequestHandler<GetListByDynamicOperationClaimQuery, GetListResponse<GetListOperationClaimListItemDto>>
    {
        private readonly IOperationClaimRepository _operationClaimRepository;
        private readonly IMapper _mapper;

        public GetListByDynamicOperationClaimQueryHandler(IOperationClaimRepository operationClaimRepository, IMapper mapper)
        {
            _operationClaimRepository = operationClaimRepository;
            _mapper = mapper;
        }

        public async Task<GetListResponse<GetListOperationClaimListItemDto>> Handle(
            GetListByDynamicOperationClaimQuery request,
            CancellationToken cancellationToken)
        {
            DynamicQuery dynamicQuery = request.Dynamic ?? new DynamicQuery();

            IPaginate<OperationClaim> items = await _operationClaimRepository.GetListByDynamicAsync(
                dynamicQuery,
                index: request.PageRequest.PageIndex,
                size: request.PageRequest.PageSize,
                cancellationToken: cancellationToken,
                enableTracking: false);

            return _mapper.Map<GetListResponse<GetListOperationClaimListItemDto>>(items);
        }
    }
}
