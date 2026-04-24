using Application.Features.Bags.Constants;
using Application.Features.Bags.Rules;
using Application.Services.Repositories;
using AutoMapper;
using Domain.Entities;
using MediatR;
using NArchitecture.Core.Application.Pipelines.Authorization;
using NArchitecture.Core.Application.Pipelines.Caching;
using NArchitecture.Core.Application.Pipelines.Logging;
using NArchitecture.Core.Application.Pipelines.Transaction;
using static Application.Features.Bags.Constants.BagsOperationClaims;

namespace Application.Features.Bags.Commands.UpdateRange;

public class UpdateBagRangeCommand : IRequest<UpdatedBagRangeResponse>, ISecuredRequest, ICacheRemoverRequest, ILoggableRequest, ITransactionalRequest
{
    public ICollection<UpdateBagRangeItem> Items { get; set; } = new List<UpdateBagRangeItem>();

    public string[] Roles => [Admin, Write, BagsOperationClaims.UpdateRange];

    public bool BypassCache { get; }
    public string? CacheKey { get; }
    public string[]? CacheGroupKey => ["GetBags"];

    public class UpdateBagRangeItem
    {
        public Guid Id { get; set; }
        public Guid SessionId { get; set; }
        public int BagNumber { get; set; }
        public double VolumeMl { get; set; }
        public double SourceVolumeMl { get; set; }
        public double Cd34PerKg { get; set; }
        public double Cd3PerKg { get; set; }
        public Domain.Enums.BagStatus Status { get; set; }
        public Domain.Enums.BagPurpose Purpose { get; set; }
        public Guid? SplitBatchId { get; set; }
        public Guid? SlotId { get; set; }
    }

    public class UpdateBagRangeCommandHandler : IRequestHandler<UpdateBagRangeCommand, UpdatedBagRangeResponse>
    {
        private readonly IMapper _mapper;
        private readonly IBagRepository _bagRepository;
        private readonly BagBusinessRules _bagBusinessRules;

        public UpdateBagRangeCommandHandler(IMapper mapper, IBagRepository bagRepository, BagBusinessRules bagBusinessRules)
        {
            _mapper = mapper;
            _bagRepository = bagRepository;
            _bagBusinessRules = bagBusinessRules;
        }

        public async Task<UpdatedBagRangeResponse> Handle(UpdateBagRangeCommand request, CancellationToken cancellationToken)
        {
            List<Bag> items = new List<Bag>();

            foreach (UpdateBagRangeItem item in request.Items)
            {
                Bag? entity = await _bagRepository.GetAsync(
                    predicate: x => x.Id == item.Id,
                    cancellationToken: cancellationToken
                );
                await _bagBusinessRules.BagShouldExistWhenSelected(entity);
                _mapper.Map(source: item, destination: entity!);
                items.Add(entity!);
            }

            ICollection<Bag> updated = await _bagRepository.UpdateRangeAsync(items);

            return new UpdatedBagRangeResponse { Ids = updated.Select(e => e.Id).ToList() };
        }
    }
}