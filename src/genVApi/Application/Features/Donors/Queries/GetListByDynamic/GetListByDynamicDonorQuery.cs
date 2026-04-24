using Application.Features.Donors.Constants;
using Application.Features.Donors.Queries.GetList;
using Application.Services.Repositories;
using AutoMapper;
using Domain.Entities;
using NArchitecture.Core.Application.Pipelines.Authorization;
using MediatR;
using static Application.Features.Donors.Constants.DonorsOperationClaims;
using NArchitecture.Core.Application.Requests;
using NArchitecture.Core.Application.Responses;
using NArchitecture.Core.Persistence.Dynamic;
using NArchitecture.Core.Persistence.Paging;

namespace Application.Features.Donors.Queries.GetListByDynamic;

public class GetListByDynamicDonorQuery : IRequest<GetListResponse<GetListDonorListItemDto>>, ISecuredRequest
{
    public PageRequest PageRequest { get; set; }
    public DynamicQuery? Dynamic { get; set; }

    public string[] Roles => [Admin, Read];

    public class GetListByDynamicDonorQueryHandler : IRequestHandler<GetListByDynamicDonorQuery, GetListResponse<GetListDonorListItemDto>>
    {
        private readonly IDonorRepository _donorRepository;
        private readonly IMapper _mapper;

        public GetListByDynamicDonorQueryHandler(IDonorRepository donorRepository, IMapper mapper)
        {
            _donorRepository = donorRepository;
            _mapper = mapper;
        }

        public async Task<GetListResponse<GetListDonorListItemDto>> Handle(GetListByDynamicDonorQuery request, CancellationToken cancellationToken)
        {
            DynamicQuery dynamicQuery = request.Dynamic ?? new DynamicQuery();

            IPaginate<Donor> donors = await _donorRepository.GetListByDynamicAsync(
                dynamicQuery,
                index: request.PageRequest.PageIndex,
                size: request.PageRequest.PageSize,
                cancellationToken: cancellationToken,
                enableTracking: false);

            GetListResponse<GetListDonorListItemDto> response = _mapper.Map<GetListResponse<GetListDonorListItemDto>>(donors);
            return response;
        }
    }
}
