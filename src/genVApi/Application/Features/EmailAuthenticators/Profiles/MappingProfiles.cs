using Application.Features.EmailAuthenticators.Queries.GetList;
using AutoMapper;
using Domain.Entities;
using NArchitecture.Core.Application.Responses;
using NArchitecture.Core.Persistence.Paging;

namespace Application.Features.EmailAuthenticators.Profiles;

public class MappingProfiles : Profile
{
    public MappingProfiles()
    {
        CreateMap<EmailAuthenticator, GetListEmailAuthenticatorListItemDto>().ReverseMap();
        CreateMap<IPaginate<EmailAuthenticator>, GetListResponse<GetListEmailAuthenticatorListItemDto>>().ReverseMap();
    }
}
