using Application.Features.CollectionSessions.Commands.Create;
using Application.Features.CollectionSessions.Commands.CreateRange;
using Application.Features.CollectionSessions.Commands.Delete;
using Application.Features.CollectionSessions.Commands.Update;
using Application.Features.CollectionSessions.Commands.UpdateRange;
using Application.Features.CollectionSessions.Queries.GetById;
using Application.Features.CollectionSessions.Queries.GetList;
using AutoMapper;
using NArchitecture.Core.Application.Responses;
using Domain.Entities;
using NArchitecture.Core.Persistence.Paging;

namespace Application.Features.CollectionSessions.Profiles;

public class MappingProfiles : Profile
{
    public MappingProfiles()
    {
        CreateMap<CollectionSession, CreateCollectionSessionCommand>().ReverseMap();
        CreateMap<CreateCollectionSessionRangeCommand.CreateCollectionSessionRangeItem, CollectionSession>();
        CreateMap<CollectionSession, CreatedCollectionSessionResponse>().ReverseMap();
        CreateMap<CollectionSession, UpdateCollectionSessionCommand>().ReverseMap();
        CreateMap<UpdateCollectionSessionRangeCommand.UpdateCollectionSessionRangeItem, CollectionSession>();
        CreateMap<CollectionSession, UpdatedCollectionSessionResponse>().ReverseMap();
        CreateMap<CollectionSession, DeleteCollectionSessionCommand>().ReverseMap();
        CreateMap<CollectionSession, DeletedCollectionSessionResponse>().ReverseMap();
        CreateMap<CollectionSession, GetByIdCollectionSessionResponse>().ReverseMap();
        CreateMap<CollectionSession, GetListCollectionSessionListItemDto>().ReverseMap();
        CreateMap<IPaginate<CollectionSession>, GetListResponse<GetListCollectionSessionListItemDto>>().ReverseMap();
    }
}