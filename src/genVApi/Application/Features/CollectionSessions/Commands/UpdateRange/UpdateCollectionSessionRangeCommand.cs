using Application.Features.CollectionSessions.Constants;
using Application.Features.CollectionSessions.Rules;
using Application.Services.Repositories;
using AutoMapper;
using Domain.Entities;
using MediatR;
using NArchitecture.Core.Application.Pipelines.Authorization;
using NArchitecture.Core.Application.Pipelines.Caching;
using NArchitecture.Core.Application.Pipelines.Logging;
using NArchitecture.Core.Application.Pipelines.Transaction;
using static Application.Features.CollectionSessions.Constants.CollectionSessionsOperationClaims;

namespace Application.Features.CollectionSessions.Commands.UpdateRange;

public class UpdateCollectionSessionRangeCommand : IRequest<UpdatedCollectionSessionRangeResponse>, ISecuredRequest, ICacheRemoverRequest, ILoggableRequest, ITransactionalRequest
{
    public ICollection<UpdateCollectionSessionRangeItem> Items { get; set; } = new List<UpdateCollectionSessionRangeItem>();

    public string[] Roles => [Admin, Write, CollectionSessionsOperationClaims.UpdateRange];

    public bool BypassCache { get; }
    public string? CacheKey { get; }
    public string[]? CacheGroupKey => ["GetCollectionSessions"];

    public class UpdateCollectionSessionRangeItem
    {
        public Guid Id { get; set; }
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

    public class UpdateCollectionSessionRangeCommandHandler : IRequestHandler<UpdateCollectionSessionRangeCommand, UpdatedCollectionSessionRangeResponse>
    {
        private readonly IMapper _mapper;
        private readonly ICollectionSessionRepository _collectionSessionRepository;
        private readonly CollectionSessionBusinessRules _collectionSessionBusinessRules;

        public UpdateCollectionSessionRangeCommandHandler(IMapper mapper, ICollectionSessionRepository collectionSessionRepository, CollectionSessionBusinessRules collectionSessionBusinessRules)
        {
            _mapper = mapper;
            _collectionSessionRepository = collectionSessionRepository;
            _collectionSessionBusinessRules = collectionSessionBusinessRules;
        }

        public async Task<UpdatedCollectionSessionRangeResponse> Handle(UpdateCollectionSessionRangeCommand request, CancellationToken cancellationToken)
        {
            List<CollectionSession> items = new List<CollectionSession>();

            foreach (UpdateCollectionSessionRangeItem item in request.Items)
            {
                CollectionSession? entity = await _collectionSessionRepository.GetAsync(
                    predicate: x => x.Id == item.Id,
                    cancellationToken: cancellationToken
                );
                await _collectionSessionBusinessRules.CollectionSessionShouldExistWhenSelected(entity);
                _mapper.Map(source: item, destination: entity!);
                items.Add(entity!);
            }

            ICollection<CollectionSession> updated = await _collectionSessionRepository.UpdateRangeAsync(items);

            return new UpdatedCollectionSessionRangeResponse { Ids = updated.Select(e => e.Id).ToList() };
        }
    }
}