using Application.Features.EmailAuthenticators.Constants;
using Application.Services.Repositories;
using AutoMapper;
using Domain.Entities;
using MediatR;
using NArchitecture.Core.Application.Pipelines.Authorization;
using NArchitecture.Core.Application.Requests;
using NArchitecture.Core.Application.Responses;
using NArchitecture.Core.Persistence.Paging;

namespace Application.Features.EmailAuthenticators.Queries.GetList;

public class GetListEmailAuthenticatorQuery : IRequest<GetListResponse<GetListEmailAuthenticatorListItemDto>>, ISecuredRequest
{
    public PageRequest PageRequest { get; set; }

    public string[] Roles => [EmailAuthenticatorsOperationClaims.Read];

    public GetListEmailAuthenticatorQuery()
    {
        PageRequest = new PageRequest { PageIndex = 0, PageSize = 10 };
    }

    public GetListEmailAuthenticatorQuery(PageRequest pageRequest)
    {
        PageRequest = pageRequest;
    }

    public class GetListEmailAuthenticatorQueryHandler : IRequestHandler<GetListEmailAuthenticatorQuery, GetListResponse<GetListEmailAuthenticatorListItemDto>>
    {
        private readonly IEmailAuthenticatorRepository _emailAuthenticatorRepository;
        private readonly IMapper _mapper;

        public GetListEmailAuthenticatorQueryHandler(IEmailAuthenticatorRepository emailAuthenticatorRepository, IMapper mapper)
        {
            _emailAuthenticatorRepository = emailAuthenticatorRepository;
            _mapper = mapper;
        }

        public async Task<GetListResponse<GetListEmailAuthenticatorListItemDto>> Handle(
            GetListEmailAuthenticatorQuery request,
            CancellationToken cancellationToken)
        {
            IPaginate<EmailAuthenticator> items = await _emailAuthenticatorRepository.GetListAsync(
                index: request.PageRequest.PageIndex,
                size: request.PageRequest.PageSize,
                enableTracking: false,
                cancellationToken: cancellationToken);

            return _mapper.Map<GetListResponse<GetListEmailAuthenticatorListItemDto>>(items);
        }
    }
}
