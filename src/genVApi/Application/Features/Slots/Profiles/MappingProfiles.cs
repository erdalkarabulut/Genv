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
        CreateMap<BagCell, CreateSlotCommand>().ReverseMap();
        CreateMap<CreateSlotRangeCommand.CreateSlotRangeItem, BagCell>();
        CreateMap<BagCell, CreatedSlotResponse>().ReverseMap();
        CreateMap<BagCell, UpdateSlotCommand>().ReverseMap();
        CreateMap<UpdateSlotRangeCommand.UpdateSlotRangeItem, BagCell>();
        CreateMap<BagCell, UpdatedSlotResponse>().ReverseMap();
        CreateMap<BagCell, DeleteSlotCommand>().ReverseMap();
        CreateMap<BagCell, DeletedSlotResponse>().ReverseMap();
        CreateMap<BagCell, GetByIdSlotResponse>().ReverseMap();
        CreateMap<BagCell, GetListSlotListItemDto>().ReverseMap();
        CreateMap<IPaginate<BagCell>, GetListResponse<GetListSlotListItemDto>>().ReverseMap();
    }
}