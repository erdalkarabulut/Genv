using Application.Features.CollectionSessions.Constants;
using Application.Features.CollectionSessions.Rules;
using Application.Services.ClinicalConfiguration;
using Application.Services.RealTime;
using Application.Services.Repositories;
using AutoMapper;
using Domain.Entities;
using NArchitecture.Core.Application.Pipelines.Authorization;
using NArchitecture.Core.Application.Pipelines.Caching;
using NArchitecture.Core.Application.Pipelines.Logging;
using NArchitecture.Core.Application.Pipelines.Transaction;
using MediatR;
using static Application.Features.CollectionSessions.Constants.CollectionSessionsOperationClaims;

namespace Application.Features.CollectionSessions.Commands.Create;

public class CreateCollectionSessionCommand : IRequest<CreatedCollectionSessionResponse>, ISecuredRequest, ICacheRemoverRequest, ILoggableRequest, ITransactionalRequest
{
    public Guid PatientId { get; set; }
    public int Day { get; set; }
    public DateTime Date { get; set; }
    public double? WbcPre { get; set; }
    public double? Hgb { get; set; }
    public double? Hct { get; set; }
    public double? Plt { get; set; }
    public double? PreCd45Percent { get; set; }
    public double? PreCd34Percent { get; set; }
    public double? PreMhs { get; set; }
    public double? WbcPost { get; set; }
    public double? HgbPost { get; set; }
    public double? HctPost { get; set; }
    public double? PltPost { get; set; }
    public double VolumeMl { get; set; }
    public double WBC { get; set; }
    public double Cd34Percent { get; set; }
    public double Cd45Percent { get; set; }
    public double Cd3Percent { get; set; }
    public double? LymphocytePercent { get; set; }
    public double? Mhs { get; set; }
    public double Cd34PerKg { get; set; }
    public double Cd3PerKg { get; set; }

    public string[] Roles => [Admin, Write, CollectionSessionsOperationClaims.Create];

    public bool BypassCache { get; }
    public string? CacheKey { get; }
    public string[]? CacheGroupKey => ["GetCollectionSessions", "GetPatients", "Dashboard"];

    public class CreateCollectionSessionCommandHandler : IRequestHandler<CreateCollectionSessionCommand, CreatedCollectionSessionResponse>
    {
        private readonly IMapper _mapper;
        private readonly ICollectionSessionRepository _collectionSessionRepository;
        private readonly IPatientRepository _patientRepository;
        private readonly CollectionSessionBusinessRules _collectionSessionBusinessRules;
        private readonly IClinicalThresholdsAccessor _clinicalThresholdsAccessor;
        private readonly IRealTimeNotifier _realTimeNotifier;

        public CreateCollectionSessionCommandHandler(IMapper mapper,
                                         ICollectionSessionRepository collectionSessionRepository,
                                         IPatientRepository patientRepository,
                                         CollectionSessionBusinessRules collectionSessionBusinessRules,
                                         IClinicalThresholdsAccessor clinicalThresholdsAccessor,
                                         IRealTimeNotifier realTimeNotifier)
        {
            _mapper = mapper;
            _collectionSessionRepository = collectionSessionRepository;
            _patientRepository = patientRepository;
            _collectionSessionBusinessRules = collectionSessionBusinessRules;
            _clinicalThresholdsAccessor = clinicalThresholdsAccessor;
            _realTimeNotifier = realTimeNotifier;
        }

        public async Task<CreatedCollectionSessionResponse> Handle(CreateCollectionSessionCommand request, CancellationToken cancellationToken)
        {
            await _collectionSessionBusinessRules.SessionDayShouldBeInAllowedRange(request.PatientId, request.Day, null, cancellationToken);
            await _collectionSessionBusinessRules.DaysMustBeConsecutiveForAllogeneic(request.PatientId, request.Day, cancellationToken);

            CollectionSession collectionSession = _mapper.Map<CollectionSession>(request);

            if ((collectionSession.Cd34PerKg == 0 || collectionSession.Cd3PerKg == 0)
                && collectionSession.VolumeMl > 0 && collectionSession.WBC > 0)
            {
                Patient? patient = await _patientRepository.GetAsync(
                    predicate: p => p.Id == request.PatientId,
                    enableTracking: false,
                    cancellationToken: cancellationToken
                );
                if (patient is not null && patient.WeightKg > 0)
                {
                    var t = await _clinicalThresholdsAccessor.GetAsync(cancellationToken);
                    collectionSession.Calculate(patient.WeightKg, t.SessionCd34Cd3Divisor);
                }
            }

            await _collectionSessionRepository.AddAsync(collectionSession);
            await _realTimeNotifier.DashboardUpdatedAsync(cancellationToken);

            CreatedCollectionSessionResponse response = _mapper.Map<CreatedCollectionSessionResponse>(collectionSession);
            return response;
        }
    }
}