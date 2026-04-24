using Application.Features.Racks.Commands.Create;
using Application.Features.Racks.Commands.CreateRange;
using Application.Features.Racks.Commands.Delete;
using Application.Features.Racks.Commands.Update;
using Application.Features.Racks.Commands.UpdateRange;
using Application.Features.Racks.Queries.GetById;
using Application.Features.Racks.Queries.GetList;
using AutoMapper;
using NArchitecture.Core.Application.Responses;
using Domain.Entities;
using NArchitecture.Core.Persistence.Paging;

namespace Application.Features.Racks.Profiles;

public class MappingProfiles : Profile
{
    public MappingProfiles()
    {
        CreateMap<Rack, CreateRackCommand>().ReverseMap();
        CreateMap<CreateRackRangeCommand.CreateRackRangeItem, Rack>();
        CreateMap<Rack, CreatedRackResponse>().ReverseMap();
        CreateMap<Rack, UpdateRackCommand>().ReverseMap();
        CreateMap<UpdateRackRangeCommand.UpdateRackRangeItem, Rack>();
        CreateMap<Rack, UpdatedRackResponse>().ReverseMap();
        CreateMap<Rack, DeleteRackCommand>().ReverseMap();
        CreateMap<Rack, DeletedRackResponse>().ReverseMap();
        CreateMap<Rack, GetByIdRackResponse>().ReverseMap();
        CreateMap<Rack, GetListRackListItemDto>().ReverseMap();
        CreateMap<IPaginate<Rack>, GetListResponse<GetListRackListItemDto>>().ReverseMap();
    }
}