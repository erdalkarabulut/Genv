using Application.Features.RefreshTokens.Queries.GetList;
using AutoMapper;
using Domain.Entities;
using NArchitecture.Core.Application.Responses;
using NArchitecture.Core.Persistence.Paging;

namespace Application.Features.RefreshTokens.Profiles;

public class MappingProfiles : Profile
{
    public MappingProfiles()
    {
        CreateMap<RefreshToken, GetListRefreshTokenListItemDto>().ReverseMap();
        CreateMap<IPaginate<RefreshToken>, GetListResponse<GetListRefreshTokenListItemDto>>().ReverseMap();
    }
}
