using Application.Features.Patients.Commands.Create;
using Application.Features.Patients.Commands.CreateRange;
using Application.Features.Patients.Commands.Delete;
using Application.Features.Patients.Commands.Update;
using Application.Features.Patients.Commands.UpdateRange;
using Application.Features.Patients.Queries.GetById;
using Application.Features.Patients.Queries.GetList;
using AutoMapper;
using NArchitecture.Core.Application.Responses;
using Domain.Entities;
using NArchitecture.Core.Persistence.Paging;

namespace Application.Features.Patients.Profiles;

public class MappingProfiles : Profile
{
    public MappingProfiles()
    {
        CreateMap<Patient, CreatePatientCommand>().ReverseMap();
        CreateMap<CreatePatientRangeCommand.CreatePatientRangeItem, Patient>();
        CreateMap<Patient, CreatedPatientResponse>().ReverseMap();
        CreateMap<Patient, UpdatePatientCommand>().ReverseMap();
        CreateMap<UpdatePatientRangeCommand.UpdatePatientRangeItem, Patient>();
        CreateMap<Patient, UpdatedPatientResponse>().ReverseMap();
        CreateMap<Patient, DeletePatientCommand>().ReverseMap();
        CreateMap<Patient, DeletedPatientResponse>().ReverseMap();
        CreateMap<Patient, GetByIdPatientResponse>().ReverseMap();
        CreateMap<Patient, GetListPatientListItemDto>().ReverseMap();
        CreateMap<IPaginate<Patient>, GetListResponse<GetListPatientListItemDto>>().ReverseMap();
    }
}