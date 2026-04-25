using Application.Features.RackSlots.Commands.Create;
using Application.Features.RackSlots.Commands.Delete;
using Application.Features.RackSlots.Commands.Update;
using Application.Features.RackSlots.Queries.GetById;
using Application.Features.RackSlots.Queries.GetList;
using AutoMapper;
using Domain.Entities;
using NArchitecture.Core.Application.Responses;
using NArchitecture.Core.Persistence.Paging;

namespace Application.Features.RackSlots.Profiles;

public class MappingProfiles : Profile
{
    public MappingProfiles()
    {
        CreateMap<Slot, CreateRackSlotCommand>().ReverseMap();
        CreateMap<Slot, CreatedRackSlotResponse>().ReverseMap();
        CreateMap<Slot, UpdateRackSlotCommand>().ReverseMap();
        CreateMap<Slot, UpdatedRackSlotResponse>();
        CreateMap<Slot, DeletedRackSlotResponse>();
        CreateMap<Slot, GetByIdRackSlotResponse>();
        CreateMap<Slot, GetListRackSlotListItemDto>().ReverseMap();
        CreateMap<IPaginate<Slot>, GetListResponse<GetListRackSlotListItemDto>>().ReverseMap();
    }
}
