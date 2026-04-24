using Application.Features.Boxes.Constants;
using Application.Features.Boxes.Queries.GetList;
using Application.Services.Repositories;
using AutoMapper;
using Domain.Entities;
using NArchitecture.Core.Application.Pipelines.Authorization;
using MediatR;
using static Application.Features.Boxes.Constants.BoxesOperationClaims;
using NArchitecture.Core.Application.Requests;
using NArchitecture.Core.Application.Responses;
using NArchitecture.Core.Persistence.Dynamic;
using NArchitecture.Core.Persistence.Paging;

namespace Application.Features.Boxes.Queries.GetListByDynamic;

public class GetListByDynamicBoxQuery : IRequest<GetListResponse<GetListBoxListItemDto>>, ISecuredRequest
{
    public PageRequest PageRequest { get; set; }
    public DynamicQuery? Dynamic { get; set; }

    public string[] Roles => [Admin, Read];

    public class GetListByDynamicBoxQueryHandler : IRequestHandler<GetListByDynamicBoxQuery, GetListResponse<GetListBoxListItemDto>>
    {
        private readonly IBoxRepository _boxRepository;
        private readonly IMapper _mapper;

        public GetListByDynamicBoxQueryHandler(IBoxRepository boxRepository, IMapper mapper)
        {
            _boxRepository = boxRepository;
            _mapper = mapper;
        }

        public async Task<GetListResponse<GetListBoxListItemDto>> Handle(GetListByDynamicBoxQuery request, CancellationToken cancellationToken)
        {
            DynamicQuery dynamicQuery = request.Dynamic ?? new DynamicQuery();

            IPaginate<Box> boxes = await _boxRepository.GetListByDynamicAsync(
                dynamicQuery,
                index: request.PageRequest.PageIndex,
                size: request.PageRequest.PageSize,
                cancellationToken: cancellationToken,
                enableTracking: false);

            GetListResponse<GetListBoxListItemDto> response = _mapper.Map<GetListResponse<GetListBoxListItemDto>>(boxes);
            return response;
        }
    }
}
