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

namespace Application.Features.Bags.Commands.Store;

public class StoreBagCommand : IRequest<StoreBagResponse>, ISecuredRequest, ICacheRemoverRequest, ILoggableRequest, ITransactionalRequest
{
    public Guid BagId { get; set; }
    public Guid SlotId { get; set; }

    public string[] Roles => [Admin, Write];

    public bool BypassCache { get; }
    public string? CacheKey { get; }
    public string[]? CacheGroupKey => ["GetBags", "GetSlots", "Dashboard"];

    public class StoreBagCommandHandler : IRequestHandler<StoreBagCommand, StoreBagResponse>
    {
        private readonly IBagRepository _bagRepository;
        private readonly ISlotRepository _slotRepository;
        private readonly IBagMovementRepository _movementRepository;
        private readonly BagBusinessRules _bagBusinessRules;
        private readonly IRealTimeNotifier _notifier;

        public StoreBagCommandHandler(
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

        public async Task<StoreBagResponse> Handle(StoreBagCommand request, CancellationToken cancellationToken)
        {
            Bag? bag = await _bagRepository.GetAsync(predicate: b => b.Id == request.BagId, cancellationToken: cancellationToken);
            await _bagBusinessRules.BagShouldExistWhenSelected(bag);

            if (bag!.SlotId.HasValue)
                throw new BusinessException("Bag is already stored in a slot. Move it instead.");

            Slot? slot = await _slotRepository.GetAsync(predicate: s => s.Id == request.SlotId, cancellationToken: cancellationToken);
            if (slot is null)
                throw new BusinessException("Slot not found.");
            if (slot.IsOccupied)
                throw new BusinessException("Slot is already occupied.");

            slot.IsOccupied = true;
            slot.Version += 1;
            bag.SlotId = slot.Id;
            bag.Status = BagStatus.Stored;

            await _slotRepository.UpdateAsync(slot);
            await _bagRepository.UpdateAsync(bag);
            await _movementRepository.AddAsync(new BagMovement
            {
                BagId = bag.Id,
                FromSlotId = null,
                ToSlotId = slot.Id,
                Action = "Store"
            });

            await _notifier.BagStoredAsync(bag.Id, slot.Id, cancellationToken);
            await _notifier.DashboardUpdatedAsync(cancellationToken);

            return new StoreBagResponse { BagId = bag.Id, SlotId = slot.Id, Status = bag.Status };
        }
    }
}

public class StoreBagResponse
{
    public Guid BagId { get; set; }
    public Guid SlotId { get; set; }
    public BagStatus Status { get; set; }
}
