using Application.Features.Slots.Commands.Create;
using Application.Features.Slots.Commands.CreateRange;
using Application.Features.Slots.Commands.Delete;
using Application.Features.Slots.Commands.Update;
using Application.Features.Slots.Commands.UpdateRange;
using Application.Features.Slots.Queries.GetById;
using Application.Features.Slots.Queries.GetList;
using AutoMapper;
using NArchitecture.Core.Application.Responses;
using Domain.Entities;
using NArchitecture.Core.Persistence.Paging;

namespace Application.Features.Slots.Profiles;

public class MappingProfiles : Profile
{
    public MappingProfiles()
    {
        CreateMap<Slot, CreateSlotCommand>().ReverseMap();
        CreateMap<CreateSlotRangeCommand.CreateSlotRangeItem, Slot>();
        CreateMap<Slot, CreatedSlotResponse>().ReverseMap();
        CreateMap<Slot, UpdateSlotCommand>().ReverseMap();
        CreateMap<UpdateSlotRangeCommand.UpdateSlotRangeItem, Slot>();
        CreateMap<Slot, UpdatedSlotResponse>().ReverseMap();
        CreateMap<Slot, DeleteSlotCommand>().ReverseMap();
        CreateMap<Slot, DeletedSlotResponse>().ReverseMap();
        CreateMap<Slot, GetByIdSlotResponse>().ReverseMap();
        CreateMap<Slot, GetListSlotListItemDto>().ReverseMap();
        CreateMap<IPaginate<Slot>, GetListResponse<GetListSlotListItemDto>>().ReverseMap();
    }
}