using Application.Features.CollectionSessions.Constants;
using Application.Features.CollectionSessions.Rules;
using Application.Services.ClinicalConfiguration;
using Application.Services.Repositories;
using Domain.Entities;
using MediatR;
using NArchitecture.Core.Application.Pipelines.Authorization;
using NArchitecture.Core.Application.Pipelines.Caching;
using NArchitecture.Core.Application.Pipelines.Logging;
using NArchitecture.Core.Application.Pipelines.Transaction;
using NArchitecture.Core.CrossCuttingConcerns.Exception.Types;
using static Application.Features.CollectionSessions.Constants.CollectionSessionsOperationClaims;

namespace Application.Features.CollectionSessions.Commands.Calculate;

public class CalculateSessionCommand : IRequest<CalculateSessionResponse>, ISecuredRequest, ICacheRemoverRequest, ILoggableRequest, ITransactionalRequest
{
    public Guid SessionId { get; set; }

    public string[] Roles => [Admin, Write];

    public bool BypassCache { get; }
    public string? CacheKey { get; }
    public string[]? CacheGroupKey => ["GetCollectionSessions", "GetPatients", "Dashboard"];

    public class CalculateSessionCommandHandler : IRequestHandler<CalculateSessionCommand, CalculateSessionResponse>
    {
        private readonly ICollectionSessionRepository _sessionRepository;
        private readonly IPatientRepository _patientRepository;
        private readonly CollectionSessionBusinessRules _rules;
        private readonly IClinicalThresholdsAccessor _clinicalThresholdsAccessor;

        public CalculateSessionCommandHandler(
            ICollectionSessionRepository sessionRepository,
            IPatientRepository patientRepository,
            CollectionSessionBusinessRules rules,
            IClinicalThresholdsAccessor clinicalThresholdsAccessor)
        {
            _sessionRepository = sessionRepository;
            _patientRepository = patientRepository;
            _rules = rules;
            _clinicalThresholdsAccessor = clinicalThresholdsAccessor;
        }

        public async Task<CalculateSessionResponse> Handle(CalculateSessionCommand request, CancellationToken cancellationToken)
        {
            CollectionSession? session = await _sessionRepository.GetAsync(
                predicate: s => s.Id == request.SessionId,
                cancellationToken: cancellationToken
            );
            await _rules.CollectionSessionShouldExistWhenSelected(session);

            Patient? patient = await _patientRepository.GetAsync(
                predicate: p => p.Id == session!.PatientId,
                cancellationToken: cancellationToken
            );
            if (patient is null || patient.WeightKg <= 0)
                throw new BusinessException("Patient not found or invalid weight.");

            var t = await _clinicalThresholdsAccessor.GetAsync(cancellationToken);
            session!.Calculate(patient.WeightKg, t.SessionCd34Cd3Divisor);
            await _sessionRepository.UpdateAsync(session);

            return new CalculateSessionResponse
            {
                SessionId = session.Id,
                AbsoluteCellCount = session.AbsoluteCellCount,
                Cd34PerKg = session.Cd34PerKg,
                Cd3PerKg = session.Cd3PerKg,
                PatientWeightKg = patient.WeightKg
            };
        }
    }
}

public class CalculateSessionResponse
{
    public Guid SessionId { get; set; }
    public double AbsoluteCellCount { get; set; }
    public double Cd34PerKg { get; set; }
    public double Cd3PerKg { get; set; }
    public double PatientWeightKg { get; set; }
}
