using Application.Features.UserOperationClaims.Constants;
using Application.Features.UserOperationClaims.Queries.GetList;
using Application.Services.Repositories;
using AutoMapper;
using Domain.Entities;
using MediatR;
using NArchitecture.Core.Application.Pipelines.Authorization;
using NArchitecture.Core.Application.Requests;
using NArchitecture.Core.Application.Responses;
using NArchitecture.Core.Persistence.Dynamic;
using NArchitecture.Core.Persistence.Paging;

namespace Application.Features.UserOperationClaims.Queries.GetListByDynamic;

public class GetListByDynamicUserOperationClaimQuery : IRequest<GetListResponse<GetListUserOperationClaimListItemDto>>, ISecuredRequest
{
    public PageRequest PageRequest { get; set; } = null!;
    public DynamicQuery? Dynamic { get; set; }

    public string[] Roles => [UserOperationClaimsOperationClaims.Read];

    public class GetListByDynamicUserOperationClaimQueryHandler : IRequestHandler<GetListByDynamicUserOperationClaimQuery, GetListResponse<GetListUserOperationClaimListItemDto>>
    {
        private readonly IUserOperationClaimRepository _userOperationClaimRepository;
        private readonly IMapper _mapper;

        public GetListByDynamicUserOperationClaimQueryHandler(IUserOperationClaimRepository userOperationClaimRepository, IMapper mapper)
        {
            _userOperationClaimRepository = userOperationClaimRepository;
            _mapper = mapper;
        }

        public async Task<GetListResponse<GetListUserOperationClaimListItemDto>> Handle(
            GetListByDynamicUserOperationClaimQuery request,
            CancellationToken cancellationToken)
        {
            DynamicQuery dynamicQuery = request.Dynamic ?? new DynamicQuery();

            IPaginate<UserOperationClaim> items = await _userOperationClaimRepository.GetListByDynamicAsync(
                dynamicQuery,
                index: request.PageRequest.PageIndex,
                size: request.PageRequest.PageSize,
                cancellationToken: cancellationToken,
                enableTracking: false);

            return _mapper.Map<GetListResponse<GetListUserOperationClaimListItemDto>>(items);
        }
    }
}
