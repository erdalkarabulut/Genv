using Application.Features.OtpAuthenticators.Constants;
using Application.Services.Repositories;
using AutoMapper;
using Domain.Entities;
using MediatR;
using NArchitecture.Core.Application.Pipelines.Authorization;
using NArchitecture.Core.Application.Requests;
using NArchitecture.Core.Application.Responses;
using NArchitecture.Core.Persistence.Paging;

namespace Application.Features.OtpAuthenticators.Queries.GetList;

public class GetListOtpAuthenticatorQuery : IRequest<GetListResponse<GetListOtpAuthenticatorListItemDto>>, ISecuredRequest
{
    public PageRequest PageRequest { get; set; }

    public string[] Roles => [OtpAuthenticatorsOperationClaims.Read];

    public GetListOtpAuthenticatorQuery()
    {
        PageRequest = new PageRequest { PageIndex = 0, PageSize = 10 };
    }

    public GetListOtpAuthenticatorQuery(PageRequest pageRequest)
    {
        PageRequest = pageRequest;
    }

    public class GetListOtpAuthenticatorQueryHandler : IRequestHandler<GetListOtpAuthenticatorQuery, GetListResponse<GetListOtpAuthenticatorListItemDto>>
    {
        private readonly IOtpAuthenticatorRepository _otpAuthenticatorRepository;
        private readonly IMapper _mapper;

        public GetListOtpAuthenticatorQueryHandler(IOtpAuthenticatorRepository otpAuthenticatorRepository, IMapper mapper)
        {
            _otpAuthenticatorRepository = otpAuthenticatorRepository;
            _mapper = mapper;
        }

        public async Task<GetListResponse<GetListOtpAuthenticatorListItemDto>> Handle(
            GetListOtpAuthenticatorQuery request,
            CancellationToken cancellationToken)
        {
            IPaginate<OtpAuthenticator> items = await _otpAuthenticatorRepository.GetListAsync(
                index: request.PageRequest.PageIndex,
                size: request.PageRequest.PageSize,
                enableTracking: false,
                cancellationToken: cancellationToken);

            return _mapper.Map<GetListResponse<GetListOtpAuthenticatorListItemDto>>(items);
        }
    }
}
