using Application.Features.Boxes.Constants;
using Application.Services.Repositories;
using AutoMapper;
using Domain.Entities;
using NArchitecture.Core.Application.Pipelines.Authorization;
using NArchitecture.Core.Application.Pipelines.Caching;
using NArchitecture.Core.Application.Requests;
using NArchitecture.Core.Application.Responses;
using NArchitecture.Core.Persistence.Paging;
using MediatR;
using static Application.Features.Boxes.Constants.BoxesOperationClaims;

namespace Application.Features.Boxes.Queries.GetList;

public class GetListBoxQuery : IRequest<GetListResponse<GetListBoxListItemDto>>, ISecuredRequest, ICachableRequest
{
    public PageRequest PageRequest { get; set; }

    public string[] Roles => [Admin, Read];

    public bool BypassCache { get; }
    public string? CacheKey => $"GetListBoxes({PageRequest.PageIndex},{PageRequest.PageSize})";
    public string? CacheGroupKey => "GetBoxes";
    public TimeSpan? SlidingExpiration { get; }

    public class GetListBoxQueryHandler : IRequestHandler<GetListBoxQuery, GetListResponse<GetListBoxListItemDto>>
    {
        private readonly IBoxRepository _boxRepository;
        private readonly IMapper _mapper;

        public GetListBoxQueryHandler(IBoxRepository boxRepository, IMapper mapper)
        {
            _boxRepository = boxRepository;
            _mapper = mapper;
        }

        public async Task<GetListResponse<GetListBoxListItemDto>> Handle(GetListBoxQuery request, CancellationToken cancellationToken)
        {
            IPaginate<Box> boxes = await _boxRepository.GetListAsync(
                index: request.PageRequest.PageIndex,
                size: request.PageRequest.PageSize, 
                cancellationToken: cancellationToken
            );

            GetListResponse<GetListBoxListItemDto> response = _mapper.Map<GetListResponse<GetListBoxListItemDto>>(boxes);
            return response;
        }
    }
}