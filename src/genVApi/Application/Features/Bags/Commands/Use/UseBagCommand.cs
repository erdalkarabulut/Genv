using Application.Features.Bags.Constants;
using Application.Features.Bags.Rules;
using Application.Services.RealTime;
using Application.Services.Repositories;
using Domain.Entities;
using Domain.Enums;
using MediatR;
using NArchitecture.Core.Application.Pipelines.Authorization;
using NArchitecture.Core.Application.Pipelines.Caching;
using NArchitecture.Core.Application.Pipelines.Logging;
using NArchitecture.Core.Application.Pipelines.Transaction;
using NArchitecture.Core.CrossCuttingConcerns.Exception.Types;
using static Application.Features.Bags.Constants.BagsOperationClaims;

namespace Application.Features.Bags.Commands.Use;

public class UseBagCommand : IRequest<UseBagResponse>, ISecuredRequest, ICacheRemoverRequest, ILoggableRequest, ITransactionalRequest
{
    public Guid BagId { get; set; }

    public string[] Roles => [Admin, Write];

    public bool BypassCache { get; }
    public string? CacheKey { get; }
    public string[]? CacheGroupKey => ["GetBags", "GetSlots", "Dashboard"];

    public class UseBagCommandHandler : IRequestHandler<UseBagCommand, UseBagResponse>
    {
        private readonly IBagRepository _bagRepository;
        private readonly ISlotRepository _slotRepository;
        private readonly IBagMovementRepository _movementRepository;
        private readonly BagBusinessRules _bagBusinessRules;
        private readonly IRealTimeNotifier _notifier;

        public UseBagCommandHandler(
            IBagRepository bagRepository,
            ISlotRepository slotRepository,
            IBagMovementRepository movementRepository,
            BagBusinessRules bagBusinessRules,
            IRealTimeNotifier notifier)
        {
            _bagRepository = bagRepository;
            _slotRepository = slotRepository;
            _movementRepository = movementRepository;
            _bagBusinessRules = bagBusinessRules;
            _notifier = notifier;
        }

        public async Task<UseBagResponse> Handle(UseBagCommand request, CancellationToken cancellationToken)
        {
            Bag? bag = await _bagRepository.GetAsync(predicate: b => b.Id == request.BagId, cancellationToken: cancellationToken);
            await _bagBusinessRules.BagShouldExistWhenSelected(bag);

            if (bag!.Status == BagStatus.Used)
                throw new BusinessException("Bag is already used.");

            Guid? fromSlotId = bag.SlotId;
            if (fromSlotId.HasValue)
            {
                Slot? slot = await _slotRepository.GetAsync(predicate: s => s.Id == fromSlotId.Value, cancellationToken: cancellationToken);
                if (slot != null)
                {
                    slot.IsOccupied = false;
                    slot.Version += 1;
                    await _slotRepository.UpdateAsync(slot);
                }
                bag.SlotId = null;
            }

            bag.Status = BagStatus.Used;
            await _bagRepository.UpdateAsync(bag);

            await _movementRepository.AddAsync(new BagMovement
            {
                BagId = bag.Id,
                FromSlotId = fromSlotId,
                ToSlotId = null,
                Action = "Use"
            });

            await _notifier.BagUsedAsync(bag.Id, fromSlotId, cancellationToken);
            await _notifier.DashboardUpdatedAsync(cancellationToken);

            return new UseBagResponse { BagId = bag.Id, FreedSlotId = fromSlotId };
        }
    }
}

public class UseBagResponse
{
    public Guid BagId { get; set; }
    public Guid? FreedSlotId { get; set; }
}
