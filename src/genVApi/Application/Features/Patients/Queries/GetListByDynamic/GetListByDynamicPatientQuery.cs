using Application.Features.Patients.Constants;
using Application.Features.Patients.Queries.GetList;
using Application.Services.Repositories;
using AutoMapper;
using Domain.Entities;
using NArchitecture.Core.Application.Pipelines.Authorization;
using MediatR;
using static Application.Features.Patients.Constants.PatientsOperationClaims;
using NArchitecture.Core.Application.Requests;
using NArchitecture.Core.Application.Responses;
using NArchitecture.Core.Persistence.Dynamic;
using NArchitecture.Core.Persistence.Paging;

namespace Application.Features.Patients.Queries.GetListByDynamic;

public class GetListByDynamicPatientQuery : IRequest<GetListResponse<GetListPatientListItemDto>>, ISecuredRequest
{
    public PageRequest PageRequest { get; set; }
    public DynamicQuery? Dynamic { get; set; }

    public string[] Roles => [Admin, Read];

    public class GetListByDynamicPatientQueryHandler : IRequestHandler<GetListByDynamicPatientQuery, GetListResponse<GetListPatientListItemDto>>
    {
        private readonly IPatientRepository _patientRepository;
        private readonly IMapper _mapper;

        public GetListByDynamicPatientQueryHandler(IPatientRepository patientRepository, IMapper mapper)
        {
            _patientRepository = patientRepository;
            _mapper = mapper;
        }

        public async Task<GetListResponse<GetListPatientListItemDto>> Handle(GetListByDynamicPatientQuery request, CancellationToken cancellationToken)
        {
            DynamicQuery dynamicQuery = request.Dynamic ?? new DynamicQuery();

            IPaginate<Patient> patients = await _patientRepository.GetListByDynamicAsync(
                dynamicQuery,
                index: request.PageRequest.PageIndex,
                size: request.PageRequest.PageSize,
                cancellationToken: cancellationToken,
                enableTracking: false);

            GetListResponse<GetListPatientListItemDto> response = _mapper.Map<GetListResponse<GetListPatientListItemDto>>(patients);
            return response;
        }
    }
}
