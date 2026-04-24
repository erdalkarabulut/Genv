using Application.Features.Donors.Constants;
using Application.Services.Repositories;
using AutoMapper;
using Domain.Entities;
using NArchitecture.Core.Application.Pipelines.Authorization;
using NArchitecture.Core.Application.Pipelines.Caching;
using NArchitecture.Core.Application.Requests;
using NArchitecture.Core.Application.Responses;
using NArchitecture.Core.Persistence.Paging;
using MediatR;
using static Application.Features.Donors.Constants.DonorsOperationClaims;

namespace Application.Features.Donors.Queries.GetList;

public class GetListDonorQuery : IRequest<GetListResponse<GetListDonorListItemDto>>, ISecuredRequest, ICachableRequest
{
    public PageRequest PageRequest { get; set; }

    public string[] Roles => [Admin, Read];

    public bool BypassCache { get; }
    public string? CacheKey => $"GetListDonors({PageRequest.PageIndex},{PageRequest.PageSize})";
    public string? CacheGroupKey => "GetDonors";
    public TimeSpan? SlidingExpiration { get; }

    public class GetListDonorQueryHandler : IRequestHandler<GetListDonorQuery, GetListResponse<GetListDonorListItemDto>>
    {
        private readonly IDonorRepository _donorRepository;
        private readonly IMapper _mapper;

        public GetListDonorQueryHandler(IDonorRepository donorRepository, IMapper mapper)
        {
            _donorRepository = donorRepository;
            _mapper = mapper;
        }

        public async Task<GetListResponse<GetListDonorListItemDto>> Handle(GetListDonorQuery request, CancellationToken cancellationToken)
        {
            IPaginate<Donor> donors = await _donorRepository.GetListAsync(
                index: request.PageRequest.PageIndex,
                size: request.PageRequest.PageSize, 
                cancellationToken: cancellationToken
            );

            GetListResponse<GetListDonorListItemDto> response = _mapper.Map<GetListResponse<GetListDonorListItemDto>>(donors);
            return response;
        }
    }
}