using Application.Features.Patients.Constants;
using Application.Features.Patients.Rules;
using Application.Services.Repositories;
using Domain.Entities;
using NArchitecture.Core.Application.Pipelines.Authorization;
using NArchitecture.Core.Application.Pipelines.Caching;
using NArchitecture.Core.Application.Pipelines.Logging;
using NArchitecture.Core.Application.Pipelines.Transaction;
using MediatR;
using static Application.Features.Patients.Constants.PatientsOperationClaims;

namespace Application.Features.Patients.Commands.DeleteRange;

public class DeletePatientRangeCommand : IRequest<DeletedPatientRangeResponse>, ISecuredRequest, ICacheRemoverRequest, ILoggableRequest, ITransactionalRequest
{
    public ICollection<Guid> Ids { get; set; } = new List<Guid>();

    public string[] Roles => [Admin, Write, PatientsOperationClaims.DeleteRange];

    public bool BypassCache { get; }
    public string? CacheKey { get; }
    public string[]? CacheGroupKey => ["GetPatients"];

    public class DeletePatientRangeCommandHandler : IRequestHandler<DeletePatientRangeCommand, DeletedPatientRangeResponse>
    {
        private readonly IPatientRepository _patientRepository;
        private readonly PatientBusinessRules _patientBusinessRules;

        public DeletePatientRangeCommandHandler(IPatientRepository patientRepository, PatientBusinessRules patientBusinessRules)
        {
            _patientRepository = patientRepository;
            _patientBusinessRules = patientBusinessRules;
        }

        public async Task<DeletedPatientRangeResponse> Handle(DeletePatientRangeCommand request, CancellationToken cancellationToken)
        {
            List<Patient> patients = new List<Patient>();

            foreach (Guid id in request.Ids)
            {
                Patient? patient = await _patientRepository.GetAsync(
                    predicate: p => p.Id == id,
                    cancellationToken: cancellationToken
                );
                await _patientBusinessRules.PatientShouldExistWhenSelected(patient);
                patients.Add(patient!);
            }

            await _patientRepository.DeleteRangeAsync(patients, permanent: true);

            return new DeletedPatientRangeResponse { DeletedCount = request.Ids.Count };
        }
    }
}
