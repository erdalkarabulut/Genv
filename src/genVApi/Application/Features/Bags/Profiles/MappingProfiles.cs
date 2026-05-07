using Application.Features.Bags.Commands.Create;
using Application.Features.Bags.Commands.CreateRange;
using Application.Features.Bags.Commands.Delete;
using Application.Features.Bags.Commands.Update;
using Application.Features.Bags.Commands.UpdateRange;
using Application.Features.Bags.Queries.GetById;
using Application.Features.Bags.Queries.GetList;
using AutoMapper;
using NArchitecture.Core.Application.Responses;
using Domain.Entities;
using NArchitecture.Core.Persistence.Paging;

namespace Application.Features.Bags.Profiles;

public class MappingProfiles : Profile
{
    public MappingProfiles()
    {
        CreateMap<Bag, CreateBagCommand>().ReverseMap();
        CreateMap<CreateBagRangeCommand.CreateBagRangeItem, Bag>();
        CreateMap<Bag, CreatedBagResponse>().ReverseMap();
        CreateMap<Bag, UpdateBagCommand>().ReverseMap();
        CreateMap<UpdateBagRangeCommand.UpdateBagRangeItem, Bag>();
        CreateMap<Bag, UpdatedBagResponse>().ReverseMap();
        CreateMap<Bag, DeleteBagCommand>().ReverseMap();
        CreateMap<Bag, DeletedBagResponse>().ReverseMap();
        CreateMap<Bag, GetByIdBagResponse>().ReverseMap();
        CreateMap<Bag, GetListBagListItemDto>()
            .ForMember(d => d.BagCellLocation, opt => opt.MapFrom(s => s.BagCell != null ? s.BagCell.GetFullLocation() : null))
            .ReverseMap();
        CreateMap<IPaginate<Bag>, GetListResponse<GetListBagListItemDto>>().ReverseMap();
    }
}