using Application.Features.Donors.Commands.Create;
using Application.Features.Donors.Commands.CreateRange;
using Application.Features.Donors.Commands.Delete;
using Application.Features.Donors.Commands.Update;
using Application.Features.Donors.Commands.UpdateRange;
using Application.Features.Donors.Queries.GetById;
using Application.Features.Donors.Queries.GetList;
using AutoMapper;
using NArchitecture.Core.Application.Responses;
using Domain.Entities;
using NArchitecture.Core.Persistence.Paging;

namespace Application.Features.Donors.Profiles;

public class MappingProfiles : Profile
{
    public MappingProfiles()
    {
        CreateMap<Donor, CreateDonorCommand>().ReverseMap();
        CreateMap<CreateDonorRangeCommand.CreateDonorRangeItem, Donor>();
        CreateMap<Donor, CreatedDonorResponse>().ReverseMap();
        CreateMap<Donor, UpdateDonorCommand>().ReverseMap();
        CreateMap<UpdateDonorRangeCommand.UpdateDonorRangeItem, Donor>();
        CreateMap<Donor, UpdatedDonorResponse>().ReverseMap();
        CreateMap<Donor, DeleteDonorCommand>().ReverseMap();
        CreateMap<Donor, DeletedDonorResponse>().ReverseMap();
        CreateMap<Donor, GetByIdDonorResponse>().ReverseMap();
        CreateMap<Donor, GetListDonorListItemDto>().ReverseMap();
        CreateMap<IPaginate<Donor>, GetListResponse<GetListDonorListItemDto>>().ReverseMap();
    }
}