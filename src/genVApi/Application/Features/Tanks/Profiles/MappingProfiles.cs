using Application.Features.Tanks.Commands.Create;
using Application.Features.Tanks.Commands.CreateRange;
using Application.Features.Tanks.Commands.Delete;
using Application.Features.Tanks.Commands.Update;
using Application.Features.Tanks.Commands.UpdateRange;
using Application.Features.Tanks.Queries.GetById;
using Application.Features.Tanks.Queries.GetList;
using AutoMapper;
using NArchitecture.Core.Application.Responses;
using Domain.Entities;
using NArchitecture.Core.Persistence.Paging;

namespace Application.Features.Tanks.Profiles;

public class MappingProfiles : Profile
{
    public MappingProfiles()
    {
        CreateMap<Tank, CreateTankCommand>().ReverseMap();
        CreateMap<CreateTankRangeCommand.CreateTankRangeItem, Tank>();
        CreateMap<Tank, CreatedTankResponse>().ReverseMap();
        CreateMap<Tank, UpdateTankCommand>().ReverseMap();
        CreateMap<UpdateTankRangeCommand.UpdateTankRangeItem, Tank>();
        CreateMap<Tank, UpdatedTankResponse>().ReverseMap();
        CreateMap<Tank, DeleteTankCommand>().ReverseMap();
        CreateMap<Tank, DeletedTankResponse>().ReverseMap();
        CreateMap<Tank, GetByIdTankResponse>().ReverseMap();
        CreateMap<Tank, GetListTankListItemDto>().ReverseMap();
        CreateMap<IPaginate<Tank>, GetListResponse<GetListTankListItemDto>>().ReverseMap();
    }
}