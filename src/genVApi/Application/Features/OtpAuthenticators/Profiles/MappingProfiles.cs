using Application.Features.OtpAuthenticators.Queries.GetList;
using AutoMapper;
using Domain.Entities;
using NArchitecture.Core.Application.Responses;
using NArchitecture.Core.Persistence.Paging;

namespace Application.Features.OtpAuthenticators.Profiles;

public class MappingProfiles : Profile
{
    public MappingProfiles()
    {
        CreateMap<OtpAuthenticator, GetListOtpAuthenticatorListItemDto>().ReverseMap();
        CreateMap<IPaginate<OtpAuthenticator>, GetListResponse<GetListOtpAuthenticatorListItemDto>>().ReverseMap();
    }
}
