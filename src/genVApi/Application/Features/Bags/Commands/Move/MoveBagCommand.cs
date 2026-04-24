using Application.Features.Bags.Constants;
using Application.Features.Bags.Rules;
using Application.Services.RealTime;
using Application.Services.Repositories;
using Domain.Entities;
using MediatR;
using NArchitecture.Core.Application.Pipelines.Authorization;
using NArchitecture.Core.Application.Pipelines.Caching;
using NArchitecture.Core.Application.Pipelines.Logging;
using NArchitecture.Core.Application.Pipelines.Transaction;
using NArchitecture.Core.CrossCuttingConcerns.Exception.Types;
using static Application.Features.Bags.Constants.BagsOperationClaims;

namespace Application.Features.Bags.Commands.Move;

public class MoveBagCommand : IRequest<MoveBagResponse>, ISecuredRequest, ICacheRemoverRequest, ILoggableRequest, ITransactionalRequest
{
    public Guid BagId { get; set; }
    public Guid TargetSlotId { get; set; }

    public string[] Roles => [Admin, Write];

    public bool BypassCache { get; }
    public string? CacheKey { get; }
    public string[]? CacheGroupKey => ["GetBags", "GetSlots", "Dashboard"];

    public class MoveBagCommandHandler : IRequestHandler<MoveBagCommand, MoveBagResponse>
    {
        private readonly IBagRepository _bagRepository;
        private readonly ISlotRepository _slotRepository;
        private readonly IBagMovementRepository _movementRepository;
        private readonly BagBusinessRules _bagBusinessRules;
        private readonly IRealTimeNotifier _notifier;

        public MoveBagCommandHandler(
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

        public async Task<MoveBagResponse> Handle(MoveBagCommand request, CancellationToken cancellationToken)
        {
            Bag? bag = await _bagRepository.GetAsync(predicate: b => b.Id == request.BagId, cancellationToken: cancellationToken);
            await _bagBusinessRules.BagShouldExistWhenSelected(bag);

            if (!bag!.SlotId.HasValue)
                throw new BusinessException("Bag is not stored in any slot. Store it first.");

            Guid fromSlotId = bag.SlotId.Value;
            if (fromSlotId == request.TargetSlotId)
                return new MoveBagResponse { BagId = bag.Id, FromSlotId = fromSlotId, ToSlotId = fromSlotId };

            Slot? fromSlot = await _slotRepository.GetAsync(predicate: s => s.Id == fromSlotId, cancellationToken: cancellationToken);
            Slot? toSlot = await _slotRepository.GetAsync(predicate: s => s.Id == request.TargetSlotId, cancellationToken: cancellationToken);
            if (fromSlot is null || toSlot is null)
                throw new BusinessException("Source or target slot not found.");
            if (toSlot.IsOccupied)
                throw new BusinessException("Target slot is occupied.");

            fromSlot.IsOccupied = false;
            fromSlot.Version += 1;
            toSlot.IsOccupied = true;
            toSlot.Version += 1;
            bag.SlotId = toSlot.Id;

            await _slotRepository.UpdateAsync(fromSlot);
            await _slotRepository.UpdateAsync(toSlot);
            await _bagRepository.UpdateAsync(bag);
            await _movementRepository.AddAsync(new BagMovement
            {
                BagId = bag.Id,
                FromSlotId = fromSlot.Id,
                ToSlotId = toSlot.Id,
                Action = "Move"
            });

            await _notifier.BagMovedAsync(bag.Id, fromSlot.Id, toSlot.Id, cancellationToken);
            await _notifier.DashboardUpdatedAsync(cancellationToken);

            return new MoveBagResponse { BagId = bag.Id, FromSlotId = fromSlot.Id, ToSlotId = toSlot.Id };
        }
    }
}

public class MoveBagResponse
{
    public Guid BagId { get; set; }
    public Guid FromSlotId { get; set; }
    public Guid ToSlotId { get; set; }
}
