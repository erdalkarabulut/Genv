using Application.Features.Users.Constants;
using Application.Features.Users.Queries.GetList;
using Application.Services.Repositories;
using AutoMapper;
using Domain.Entities;
using MediatR;
using NArchitecture.Core.Application.Pipelines.Authorization;
using NArchitecture.Core.Application.Requests;
using NArchitecture.Core.Application.Responses;
using NArchitecture.Core.Persistence.Dynamic;
using NArchitecture.Core.Persistence.Paging;

namespace Application.Features.Users.Queries.GetListByDynamic;

public class GetListByDynamicUserQuery : IRequest<GetListResponse<GetListUserListItemDto>>, ISecuredRequest
{
    public PageRequest PageRequest { get; set; } = null!;
    public DynamicQuery? Dynamic { get; set; }

    public string[] Roles => [UsersOperationClaims.Read];

    public class GetListByDynamicUserQueryHandler : IRequestHandler<GetListByDynamicUserQuery, GetListResponse<GetListUserListItemDto>>
    {
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;

        public GetListByDynamicUserQueryHandler(IUserRepository userRepository, IMapper mapper)
        {
            _userRepository = userRepository;
            _mapper = mapper;
        }

        public async Task<GetListResponse<GetListUserListItemDto>> Handle(
            GetListByDynamicUserQuery request,
            CancellationToken cancellationToken)
        {
            DynamicQuery dynamicQuery = request.Dynamic ?? new DynamicQuery();

            IPaginate<User> items = await _userRepository.GetListByDynamicAsync(
                dynamicQuery,
                index: request.PageRequest.PageIndex,
                size: request.PageRequest.PageSize,
                cancellationToken: cancellationToken,
                enableTracking: false);

            return _mapper.Map<GetListResponse<GetListUserListItemDto>>(items);
        }
    }
}
