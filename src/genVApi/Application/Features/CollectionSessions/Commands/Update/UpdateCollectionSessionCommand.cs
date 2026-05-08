using Application.Features.CollectionSessions.Constants;
using Application.Features.CollectionSessions.Rules;
using Application.Services.Repositories;
using AutoMapper;
using Domain.Entities;
using NArchitecture.Core.Application.Pipelines.Authorization;
using NArchitecture.Core.Application.Pipelines.Caching;
using NArchitecture.Core.Application.Pipelines.Logging;
using NArchitecture.Core.Application.Pipelines.Transaction;
using MediatR;
using static Application.Features.CollectionSessions.Constants.CollectionSessionsOperationClaims;

namespace Application.Features.CollectionSessions.Commands.Update;

public class UpdateCollectionSessionCommand : IRequest<UpdatedCollectionSessionResponse>, ISecuredRequest, ICacheRemoverRequest, ILoggableRequest, ITransactionalRequest
{
    public Guid Id { get; set; }
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
    public double AbsoluteCellCount { get; set; }
    public double Cd34PerKg { get; set; }
    public double Cd3PerKg { get; set; }

    public string[] Roles => [Admin, Write, CollectionSessionsOperationClaims.Update];

    public bool BypassCache { get; }
    public string? CacheKey { get; }
    public string[]? CacheGroupKey => ["GetCollectionSessions", "GetPatients", "Dashboard"];

    public class UpdateCollectionSessionCommandHandler : IRequestHandler<UpdateCollectionSessionCommand, UpdatedCollectionSessionResponse>
    {
        private readonly IMapper _mapper;
        private readonly ICollectionSessionRepository _collectionSessionRepository;
        private readonly CollectionSessionBusinessRules _collectionSessionBusinessRules;

        public UpdateCollectionSessionCommandHandler(IMapper mapper, ICollectionSessionRepository collectionSessionRepository,
                                         CollectionSessionBusinessRules collectionSessionBusinessRules)
        {
            _mapper = mapper;
            _collectionSessionRepository = collectionSessionRepository;
            _collectionSessionBusinessRules = collectionSessionBusinessRules;
        }

        public async Task<UpdatedCollectionSessionResponse> Handle(UpdateCollectionSessionCommand request, CancellationToken cancellationToken)
        {
            CollectionSession? collectionSession = await _collectionSessionRepository.GetAsync(predicate: x => x.Id == request.Id, cancellationToken: cancellationToken);
            await _collectionSessionBusinessRules.CollectionSessionShouldExistWhenSelected(collectionSession);
            collectionSession = _mapper.Map(request, collectionSession);

            await _collectionSessionRepository.UpdateAsync(collectionSession!);

            UpdatedCollectionSessionResponse response = _mapper.Map<UpdatedCollectionSessionResponse>(collectionSession);
            return response;
        }
    }
}