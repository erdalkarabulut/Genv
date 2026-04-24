using Application.Features.BagMovements.Commands.Create;
using Application.Features.BagMovements.Commands.CreateRange;
using Application.Features.BagMovements.Commands.Delete;
using Application.Features.BagMovements.Commands.Update;
using Application.Features.BagMovements.Commands.UpdateRange;
using Application.Features.BagMovements.Queries.GetById;
using Application.Features.BagMovements.Queries.GetList;
using AutoMapper;
using NArchitecture.Core.Application.Responses;
using Domain.Entities;
using NArchitecture.Core.Persistence.Paging;

namespace Application.Features.BagMovements.Profiles;

public class MappingProfiles : Profile
{
    public MappingProfiles()
    {
        CreateMap<BagMovement, CreateBagMovementCommand>().ReverseMap();
        CreateMap<CreateBagMovementRangeCommand.CreateBagMovementRangeItem, BagMovement>();
        CreateMap<BagMovement, CreatedBagMovementResponse>().ReverseMap();
        CreateMap<BagMovement, UpdateBagMovementCommand>().ReverseMap();
        CreateMap<UpdateBagMovementRangeCommand.UpdateBagMovementRangeItem, BagMovement>();
        CreateMap<BagMovement, UpdatedBagMovementResponse>().ReverseMap();
        CreateMap<BagMovement, DeleteBagMovementCommand>().ReverseMap();
        CreateMap<BagMovement, DeletedBagMovementResponse>().ReverseMap();
        CreateMap<BagMovement, GetByIdBagMovementResponse>().ReverseMap();
        CreateMap<BagMovement, GetListBagMovementListItemDto>().ReverseMap();
        CreateMap<IPaginate<BagMovement>, GetListResponse<GetListBagMovementListItemDto>>().ReverseMap();
    }
}