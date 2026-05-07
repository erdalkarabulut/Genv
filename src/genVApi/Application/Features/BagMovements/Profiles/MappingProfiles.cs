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
        CreateMap<BagMovement, GetByIdBagMovementResponse>()
            .ForMember(d => d.PatientId, opt => opt.MapFrom(s => s.Bag != null && s.Bag.Session != null && s.Bag.Session.Patient != null ? s.Bag.Session.Patient.Id : (Guid?)null))
            .ForMember(d => d.PatientFullName, opt => opt.MapFrom(s => s.Bag != null && s.Bag.Session != null && s.Bag.Session.Patient != null ? s.Bag.Session.Patient.FullName : null))
            .ForMember(d => d.UsedAt, opt => opt.MapFrom(s => s.UsedAt))
            .ForMember(d => d.ActionDisplay, opt => opt.MapFrom(s => s.Action != null && s.Action.Contains(":") ? s.Action.Replace(":", " (") + ")" : s.Action))
            .ReverseMap();

        CreateMap<BagMovement, GetListBagMovementListItemDto>()
            .ForMember(d => d.PatientId, opt => opt.MapFrom(s => s.Bag != null && s.Bag.Session != null && s.Bag.Session.Patient != null ? s.Bag.Session.Patient.Id : (Guid?)null))
            .ForMember(d => d.PatientFullName, opt => opt.MapFrom(s => s.Bag != null && s.Bag.Session != null && s.Bag.Session.Patient != null ? s.Bag.Session.Patient.FullName : null))
            .ForMember(d => d.UsedAt, opt => opt.MapFrom(s => s.UsedAt))
            .ForMember(d => d.ActionDisplay, opt => opt.MapFrom(s => s.Action != null && s.Action.Contains(":") ? s.Action.Replace(":", " (") + ")" : s.Action))
            .ForMember(d => d.FromBagCellLocation, opt => opt.MapFrom(s => s.FromBagCell != null ? s.FromBagCell.GetFullLocation() : null))
            .ForMember(d => d.ToBagCellLocation, opt => opt.MapFrom(s => s.ToBagCell != null ? s.ToBagCell.GetFullLocation() : null))
            .ReverseMap();
        CreateMap<IPaginate<BagMovement>, GetListResponse<GetListBagMovementListItemDto>>().ReverseMap();
    }
}