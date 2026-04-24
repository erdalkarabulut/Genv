using Application.Features.OtpAuthenticators.Constants;
using Application.Features.OtpAuthenticators.Queries.GetList;
using Application.Services.Repositories;
using AutoMapper;
using Domain.Entities;
using MediatR;
using NArchitecture.Core.Application.Pipelines.Authorization;
using NArchitecture.Core.Application.Requests;
using NArchitecture.Core.Application.Responses;
using NArchitecture.Core.Persistence.Dynamic;
using NArchitecture.Core.Persistence.Paging;

namespace Application.Features.OtpAuthenticators.Queries.GetListByDynamic;

public class GetListByDynamicOtpAuthenticatorQuery : IRequest<GetListResponse<GetListOtpAuthenticatorListItemDto>>, ISecuredRequest
{
    public PageRequest PageRequest { get; set; } = null!;
    public DynamicQuery? Dynamic { get; set; }

    public string[] Roles => [OtpAuthenticatorsOperationClaims.Read];

    public class GetListByDynamicOtpAuthenticatorQueryHandler : IRequestHandler<GetListByDynamicOtpAuthenticatorQuery, GetListResponse<GetListOtpAuthenticatorListItemDto>>
    {
        private readonly IOtpAuthenticatorRepository _otpAuthenticatorRepository;
        private readonly IMapper _mapper;

        public GetListByDynamicOtpAuthenticatorQueryHandler(IOtpAuthenticatorRepository otpAuthenticatorRepository, IMapper mapper)
        {
            _otpAuthenticatorRepository = otpAuthenticatorRepository;
            _mapper = mapper;
        }

        public async Task<GetListResponse<GetListOtpAuthenticatorListItemDto>> Handle(
            GetListByDynamicOtpAuthenticatorQuery request,
            CancellationToken cancellationToken)
        {
            DynamicQuery dynamicQuery = request.Dynamic ?? new DynamicQuery();

            IPaginate<OtpAuthenticator> items = await _otpAuthenticatorRepository.GetListByDynamicAsync(
                dynamicQuery,
                index: request.PageRequest.PageIndex,
                size: request.PageRequest.PageSize,
                cancellationToken: cancellationToken,
                enableTracking: false);

            return _mapper.Map<GetListResponse<GetListOtpAuthenticatorListItemDto>>(items);
        }
    }
}
