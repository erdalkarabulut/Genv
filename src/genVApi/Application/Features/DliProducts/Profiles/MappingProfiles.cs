using Application.Features.DliProducts.Commands.Create;
using Application.Features.DliProducts.Commands.CreateRange;
using Application.Features.DliProducts.Commands.Delete;
using Application.Features.DliProducts.Commands.Update;
using Application.Features.DliProducts.Commands.UpdateRange;
using Application.Features.DliProducts.Queries.GetById;
using Application.Features.DliProducts.Queries.GetList;
using AutoMapper;
using NArchitecture.Core.Application.Responses;
using Domain.Entities;
using NArchitecture.Core.Persistence.Paging;

namespace Application.Features.DliProducts.Profiles;

public class MappingProfiles : Profile
{
    public MappingProfiles()
    {
        CreateMap<DliProduct, CreateDliProductCommand>().ReverseMap();
        CreateMap<CreateDliProductRangeCommand.CreateDliProductRangeItem, DliProduct>();
        CreateMap<DliProduct, CreatedDliProductResponse>().ReverseMap();
        CreateMap<DliProduct, UpdateDliProductCommand>().ReverseMap();
        CreateMap<UpdateDliProductRangeCommand.UpdateDliProductRangeItem, DliProduct>();
        CreateMap<DliProduct, UpdatedDliProductResponse>().ReverseMap();
        CreateMap<DliProduct, DeleteDliProductCommand>().ReverseMap();
        CreateMap<DliProduct, DeletedDliProductResponse>().ReverseMap();
        CreateMap<DliProduct, GetByIdDliProductResponse>().ReverseMap();
        CreateMap<DliProduct, GetListDliProductListItemDto>().ReverseMap();
        CreateMap<IPaginate<DliProduct>, GetListResponse<GetListDliProductListItemDto>>().ReverseMap();
    }
}