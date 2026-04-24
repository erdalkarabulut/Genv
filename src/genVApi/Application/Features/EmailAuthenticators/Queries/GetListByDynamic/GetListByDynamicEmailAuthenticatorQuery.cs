using Application.Features.EmailAuthenticators.Constants;
using Application.Features.EmailAuthenticators.Queries.GetList;
using Application.Services.Repositories;
using AutoMapper;
using Domain.Entities;
using MediatR;
using NArchitecture.Core.Application.Pipelines.Authorization;
using NArchitecture.Core.Application.Requests;
using NArchitecture.Core.Application.Responses;
using NArchitecture.Core.Persistence.Dynamic;
using NArchitecture.Core.Persistence.Paging;

namespace Application.Features.EmailAuthenticators.Queries.GetListByDynamic;

public class GetListByDynamicEmailAuthenticatorQuery : IRequest<GetListResponse<GetListEmailAuthenticatorListItemDto>>, ISecuredRequest
{
    public PageRequest PageRequest { get; set; } = null!;
    public DynamicQuery? Dynamic { get; set; }

    public string[] Roles => [EmailAuthenticatorsOperationClaims.Read];

    public class GetListByDynamicEmailAuthenticatorQueryHandler : IRequestHandler<GetListByDynamicEmailAuthenticatorQuery, GetListResponse<GetListEmailAuthenticatorListItemDto>>
    {
        private readonly IEmailAuthenticatorRepository _emailAuthenticatorRepository;
        private readonly IMapper _mapper;

        public GetListByDynamicEmailAuthenticatorQueryHandler(IEmailAuthenticatorRepository emailAuthenticatorRepository, IMapper mapper)
        {
            _emailAuthenticatorRepository = emailAuthenticatorRepository;
            _mapper = mapper;
        }

        public async Task<GetListResponse<GetListEmailAuthenticatorListItemDto>> Handle(
            GetListByDynamicEmailAuthenticatorQuery request,
            CancellationToken cancellationToken)
        {
            DynamicQuery dynamicQuery = request.Dynamic ?? new DynamicQuery();

            IPaginate<EmailAuthenticator> items = await _emailAuthenticatorRepository.GetListByDynamicAsync(
                dynamicQuery,
                index: request.PageRequest.PageIndex,
                size: request.PageRequest.PageSize,
                cancellationToken: cancellationToken,
                enableTracking: false);

            return _mapper.Map<GetListResponse<GetListEmailAuthenticatorListItemDto>>(items);
        }
    }
}
