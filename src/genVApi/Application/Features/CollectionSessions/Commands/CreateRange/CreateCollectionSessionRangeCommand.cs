using Application.Features.CollectionSessions.Constants;
using Application.Services.Repositories;
using AutoMapper;
using Domain.Entities;
using MediatR;
using NArchitecture.Core.Application.Pipelines.Authorization;
using NArchitecture.Core.Application.Pipelines.Caching;
using NArchitecture.Core.Application.Pipelines.Logging;
using NArchitecture.Core.Application.Pipelines.Transaction;
using static Application.Features.CollectionSessions.Constants.CollectionSessionsOperationClaims;

namespace Application.Features.CollectionSessions.Commands.CreateRange;

public class CreateCollectionSessionRangeCommand : IRequest<CreatedCollectionSessionRangeResponse>, ISecuredRequest, ICacheRemoverRequest, ILoggableRequest, ITransactionalRequest
{
    public ICollection<CreateCollectionSessionRangeItem> Items { get; set; } = new List<CreateCollectionSessionRangeItem>();

    public string[] Roles => [Admin, Write, CollectionSessionsOperationClaims.CreateRange];

    public bool BypassCache { get; }
    public string? CacheKey { get; }
    public string[]? CacheGroupKey => ["GetCollectionSessions"];

    public class CreateCollectionSessionRangeItem
    {
        public Guid PatientId { get; set; }
        public int Day { get; set; }
        public DateTime Date { get; set; }
        public double? WbcPre { get; set; }
        public double? Hgb { get; set; }
        public double? Hct { get; set; }
        public double? Plt { get; set; }
        public double VolumeMl { get; set; }
        public double WBC { get; set; }
        public double Cd34Percent { get; set; }
        public double Cd45Percent { get; set; }
        public double Cd3Percent { get; set; }
        public double? LymphocytePercent { get; set; }
        public double? Mhs { get; set; }
        public double Cd34PerKg { get; set; }
        public double Cd3PerKg { get; set; }
    }

    public class CreateCollectionSessionRangeCommandHandler : IRequestHandler<CreateCollectionSessionRangeCommand, CreatedCollectionSessionRangeResponse>
    {
        private readonly IMapper _mapper;
        private readonly ICollectionSessionRepository _collectionSessionRepository;

        public CreateCollectionSessionRangeCommandHandler(IMapper mapper, ICollectionSessionRepository collectionSessionRepository)
        {
            _mapper = mapper;
            _collectionSessionRepository = collectionSessionRepository;
        }

        public async Task<CreatedCollectionSessionRangeResponse> Handle(CreateCollectionSessionRangeCommand request, CancellationToken cancellationToken)
        {
            List<CollectionSession> collectionSessions = request.Items.Select(item => _mapper.Map<CollectionSession>(item)).ToList();

            ICollection<CollectionSession> addedCollectionSessions = await _collectionSessionRepository.AddRangeAsync(collectionSessions);

            return new CreatedCollectionSessionRangeResponse { Ids = addedCollectionSessions.Select(e => e.Id).ToList() };
        }
    }
}