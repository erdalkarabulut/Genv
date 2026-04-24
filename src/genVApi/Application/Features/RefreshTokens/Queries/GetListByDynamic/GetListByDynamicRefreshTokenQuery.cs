using Application.Features.RefreshTokens.Constants;
using Application.Features.RefreshTokens.Queries.GetList;
using Application.Services.Repositories;
using AutoMapper;
using Domain.Entities;
using MediatR;
using NArchitecture.Core.Application.Pipelines.Authorization;
using NArchitecture.Core.Application.Requests;
using NArchitecture.Core.Application.Responses;
using NArchitecture.Core.Persistence.Dynamic;
using NArchitecture.Core.Persistence.Paging;

namespace Application.Features.RefreshTokens.Queries.GetListByDynamic;

public class GetListByDynamicRefreshTokenQuery : IRequest<GetListResponse<GetListRefreshTokenListItemDto>>, ISecuredRequest
{
    public PageRequest PageRequest { get; set; } = null!;
    public DynamicQuery? Dynamic { get; set; }

    public string[] Roles => [RefreshTokensOperationClaims.Read];

    public class GetListByDynamicRefreshTokenQueryHandler : IRequestHandler<GetListByDynamicRefreshTokenQuery, GetListResponse<GetListRefreshTokenListItemDto>>
    {
        private readonly IRefreshTokenRepository _refreshTokenRepository;
        private readonly IMapper _mapper;

        public GetListByDynamicRefreshTokenQueryHandler(IRefreshTokenRepository refreshTokenRepository, IMapper mapper)
        {
            _refreshTokenRepository = refreshTokenRepository;
            _mapper = mapper;
        }

        public async Task<GetListResponse<GetListRefreshTokenListItemDto>> Handle(
            GetListByDynamicRefreshTokenQuery request,
            CancellationToken cancellationToken)
        {
            DynamicQuery dynamicQuery = request.Dynamic ?? new DynamicQuery();

            IPaginate<RefreshToken> items = await _refreshTokenRepository.GetListByDynamicAsync(
                dynamicQuery,
                index: request.PageRequest.PageIndex,
                size: request.PageRequest.PageSize,
                cancellationToken: cancellationToken,
                enableTracking: false);

            return _mapper.Map<GetListResponse<GetListRefreshTokenListItemDto>>(items);
        }
    }
}
