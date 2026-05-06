using Application.Features.Users.Constants;
using Application.Services.Repositories;
using AutoMapper;
using Domain.Entities;
using MediatR;
using NArchitecture.Core.Application.Pipelines.Authorization;
using NArchitecture.Core.Application.Requests;
using NArchitecture.Core.Application.Responses;
using NArchitecture.Core.Persistence.Paging;

namespace Application.Features.Users.Queries.GetAllClaims;

public class GetAllOperationClaimsQuery : IRequest<GetListResponse<GetAllOperationClaimsDto>>, ISecuredRequest
{
    public PageRequest PageRequest { get; set; } = new();
    public string[] Roles => new[] { UsersOperationClaims.Admin, UsersOperationClaims.Read };

    public class GetAllOperationClaimsQueryHandler : IRequestHandler<GetAllOperationClaimsQuery, GetListResponse<GetAllOperationClaimsDto>>
    {
        private readonly IOperationClaimRepository _operationClaimRepository;
        private readonly IMapper _mapper;

        public GetAllOperationClaimsQueryHandler(IOperationClaimRepository operationClaimRepository, IMapper mapper)
        {
            _operationClaimRepository = operationClaimRepository;
            _mapper = mapper;
        }

        public async Task<GetListResponse<GetAllOperationClaimsDto>> Handle(GetAllOperationClaimsQuery request, CancellationToken cancellationToken)
        {
            IPaginate<OperationClaim> operationClaims = await _operationClaimRepository.GetListAsync(
                index: request.PageRequest.PageIndex,
                size: request.PageRequest.PageSize,
                enableTracking: false,
                cancellationToken: cancellationToken
            );

            GetListResponse<GetAllOperationClaimsDto> response = _mapper.Map<GetListResponse<GetAllOperationClaimsDto>>(operationClaims);
            return response;
        }
    }
}

public class GetAllOperationClaimsDto : IResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}
