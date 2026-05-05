using Application.Features.Patients.Constants;
using Application.Services.Repositories;
using AutoMapper;
using Domain.Entities;
using MediatR;
using NArchitecture.Core.Application.Pipelines.Authorization;
using NArchitecture.Core.Application.Pipelines.Caching;
using NArchitecture.Core.Application.Pipelines.Logging;
using NArchitecture.Core.Application.Pipelines.Transaction;
using static Application.Features.Patients.Constants.PatientsOperationClaims;

namespace Application.Features.Patients.Commands.CreateRange;

public class CreatePatientRangeCommand : IRequest<CreatedPatientRangeResponse>, ISecuredRequest, ICacheRemoverRequest, ILoggableRequest, ITransactionalRequest
{
    public ICollection<CreatePatientRangeItem> Items { get; set; } = new List<CreatePatientRangeItem>();

    public string[] Roles => [Admin, Write, PatientsOperationClaims.CreateRange];

    public bool BypassCache { get; }
    public string? CacheKey { get; }
    public string[]? CacheGroupKey => ["GetPatients"];

    public class CreatePatientRangeItem
    {
        public string FullName { get; set; }
        public double WeightKg { get; set; }
        public string? BloodGroup { get; set; }
        public Domain.Enums.TransplantType TransplantType { get; set; }
        public string? Diagnosis { get; set; }
        public string? ProtocolNo { get; set; }
        public string? IdentityNumber { get; set; }
        public DateTime? BirthDate { get; set; }
        public Guid? DonorId { get; set; }
    }

    public class CreatePatientRangeCommandHandler : IRequestHandler<CreatePatientRangeCommand, CreatedPatientRangeResponse>
    {
        private readonly IMapper _mapper;
        private readonly IPatientRepository _patientRepository;

        public CreatePatientRangeCommandHandler(IMapper mapper, IPatientRepository patientRepository)
        {
            _mapper = mapper;
            _patientRepository = patientRepository;
        }

        public async Task<CreatedPatientRangeResponse> Handle(CreatePatientRangeCommand request, CancellationToken cancellationToken)
        {
            List<Patient> patients = request.Items.Select(item => _mapper.Map<Patient>(item)).ToList();

            ICollection<Patient> addedPatients = await _patientRepository.AddRangeAsync(patients);

            return new CreatedPatientRangeResponse { Ids = addedPatients.Select(e => e.Id).ToList() };
        }
    }
}