using Application.Features.Patients.Constants;
using Application.Features.Patients.Rules;
using Application.Services.Repositories;
using AutoMapper;
using Domain.Entities;
using MediatR;
using NArchitecture.Core.Application.Pipelines.Authorization;
using NArchitecture.Core.Application.Pipelines.Caching;
using NArchitecture.Core.Application.Pipelines.Logging;
using NArchitecture.Core.Application.Pipelines.Transaction;
using static Application.Features.Patients.Constants.PatientsOperationClaims;

namespace Application.Features.Patients.Commands.UpdateRange;

public class UpdatePatientRangeCommand : IRequest<UpdatedPatientRangeResponse>, ISecuredRequest, ICacheRemoverRequest, ILoggableRequest, ITransactionalRequest
{
    public ICollection<UpdatePatientRangeItem> Items { get; set; } = new List<UpdatePatientRangeItem>();

    public string[] Roles => [Admin, Write, PatientsOperationClaims.UpdateRange];

    public bool BypassCache { get; }
    public string? CacheKey { get; }
    public string[]? CacheGroupKey => ["GetPatients"];

    public class UpdatePatientRangeItem
    {
        public Guid Id { get; set; }
        public string FullName { get; set; }
        public double WeightKg { get; set; }
        public string? BloodGroup { get; set; }
        public Domain.Enums.TransplantType TransplantType { get; set; }
        public string? Diagnosis { get; set; }
        public string? ProtocolNo { get; set; }
        public DateTime? BirthDate { get; set; }
        public Guid? DonorId { get; set; }
    }

    public class UpdatePatientRangeCommandHandler : IRequestHandler<UpdatePatientRangeCommand, UpdatedPatientRangeResponse>
    {
        private readonly IMapper _mapper;
        private readonly IPatientRepository _patientRepository;
        private readonly PatientBusinessRules _patientBusinessRules;

        public UpdatePatientRangeCommandHandler(IMapper mapper, IPatientRepository patientRepository, PatientBusinessRules patientBusinessRules)
        {
            _mapper = mapper;
            _patientRepository = patientRepository;
            _patientBusinessRules = patientBusinessRules;
        }

        public async Task<UpdatedPatientRangeResponse> Handle(UpdatePatientRangeCommand request, CancellationToken cancellationToken)
        {
            List<Patient> items = new List<Patient>();

            foreach (UpdatePatientRangeItem item in request.Items)
            {
                Patient? entity = await _patientRepository.GetAsync(
                    predicate: x => x.Id == item.Id,
                    cancellationToken: cancellationToken
                );
                await _patientBusinessRules.PatientShouldExistWhenSelected(entity);
                _mapper.Map(source: item, destination: entity!);
                items.Add(entity!);
            }

            ICollection<Patient> updated = await _patientRepository.UpdateRangeAsync(items);

            return new UpdatedPatientRangeResponse { Ids = updated.Select(e => e.Id).ToList() };
        }
    }
}