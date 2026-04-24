using Application.Features.RefreshTokens.Constants;
using Application.Services.Repositories;
using AutoMapper;
using Domain.Entities;
using MediatR;
using NArchitecture.Core.Application.Pipelines.Authorization;
using NArchitecture.Core.Application.Requests;
using NArchitecture.Core.Application.Responses;
using NArchitecture.Core.Persistence.Paging;

namespace Application.Features.RefreshTokens.Queries.GetList;

public class GetListRefreshTokenQuery : IRequest<GetListResponse<GetListRefreshTokenListItemDto>>, ISecuredRequest
{
    public PageRequest PageRequest { get; set; }

    public string[] Roles => [RefreshTokensOperationClaims.Read];

    public GetListRefreshTokenQuery()
    {
        PageRequest = new PageRequest { PageIndex = 0, PageSize = 10 };
    }

    public GetListRefreshTokenQuery(PageRequest pageRequest)
    {
        PageRequest = pageRequest;
    }

    public class GetListRefreshTokenQueryHandler : IRequestHandler<GetListRefreshTokenQuery, GetListResponse<GetListRefreshTokenListItemDto>>
    {
        private readonly IRefreshTokenRepository _refreshTokenRepository;
        private readonly IMapper _mapper;

        public GetListRefreshTokenQueryHandler(IRefreshTokenRepository refreshTokenRepository, IMapper mapper)
        {
            _refreshTokenRepository = refreshTokenRepository;
            _mapper = mapper;
        }

        public async Task<GetListResponse<GetListRefreshTokenListItemDto>> Handle(
            GetListRefreshTokenQuery request,
            CancellationToken cancellationToken)
        {
            IPaginate<RefreshToken> items = await _refreshTokenRepository.GetListAsync(
                index: request.PageRequest.PageIndex,
                size: request.PageRequest.PageSize,
                enableTracking: false,
                cancellationToken: cancellationToken);

            return _mapper.Map<GetListResponse<GetListRefreshTokenListItemDto>>(items);
        }
    }
}
