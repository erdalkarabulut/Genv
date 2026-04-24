using Application.Features.Boxes.Commands.Create;
using Application.Features.Boxes.Commands.CreateRange;
using Application.Features.Boxes.Commands.Delete;
using Application.Features.Boxes.Commands.Update;
using Application.Features.Boxes.Commands.UpdateRange;
using Application.Features.Boxes.Queries.GetById;
using Application.Features.Boxes.Queries.GetList;
using AutoMapper;
using NArchitecture.Core.Application.Responses;
using Domain.Entities;
using NArchitecture.Core.Persistence.Paging;

namespace Application.Features.Boxes.Profiles;

public class MappingProfiles : Profile
{
    public MappingProfiles()
    {
        CreateMap<Box, CreateBoxCommand>().ReverseMap();
        CreateMap<CreateBoxRangeCommand.CreateBoxRangeItem, Box>();
        CreateMap<Box, CreatedBoxResponse>().ReverseMap();
        CreateMap<Box, UpdateBoxCommand>().ReverseMap();
        CreateMap<UpdateBoxRangeCommand.UpdateBoxRangeItem, Box>();
        CreateMap<Box, UpdatedBoxResponse>().ReverseMap();
        CreateMap<Box, DeleteBoxCommand>().ReverseMap();
        CreateMap<Box, DeletedBoxResponse>().ReverseMap();
        CreateMap<Box, GetByIdBoxResponse>().ReverseMap();
        CreateMap<Box, GetListBoxListItemDto>().ReverseMap();
        CreateMap<IPaginate<Box>, GetListResponse<GetListBoxListItemDto>>().ReverseMap();
    }
}