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

namespace Application.Features.Bags.Commands.Freeze;

/// <summary>
/// Mevcut bir torbayı "dondurulmuş" olarak işaretler.
/// Opsiyonel: SlotId verilirse torba önce o slota yerleştirilir, sonra Frozen yapılır.
/// </summary>
public class FreezeBagCommand : IRequest<FreezeBagResponse>, ISecuredRequest, ICacheRemoverRequest, ILoggableRequest, ITransactionalRequest
{
    public Guid BagId { get; set; }

    /// <summary>Torba henüz bir slotta değilse verilmeli. Zaten slotta ise boş bırakılır.</summary>
    public Guid? SlotId { get; set; }

    public string[] Roles => [Admin, Write];

    public bool BypassCache { get; }
    public string? CacheKey { get; }
    public string[]? CacheGroupKey => ["GetBags", "GetSlots", "Dashboard"];

    public class FreezeBagCommandHandler : IRequestHandler<FreezeBagCommand, FreezeBagResponse>
    {
        private readonly IBagRepository _bagRepository;
        private readonly ISlotRepository _slotRepository;
        private readonly IBagMovementRepository _movementRepository;
        private readonly BagBusinessRules _bagBusinessRules;
        private readonly IRealTimeNotifier _notifier;

        public FreezeBagCommandHandler(
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

        public async Task<FreezeBagResponse> Handle(FreezeBagCommand request, CancellationToken cancellationToken)
        {
            Bag? bag = await _bagRepository.GetAsync(predicate: b => b.Id == request.BagId, cancellationToken: cancellationToken);
            await _bagBusinessRules.BagShouldExistWhenSelected(bag);

            if (bag!.Status == BagStatus.Used || bag.Status == BagStatus.Discarded)
                throw new BusinessException("Kullanılmış veya iptal edilmiş torba dondurulamaz.");

            // Eğer torba henüz slotta değilse, slot zorunlu.
            Slot? targetSlot = null;
            if (!bag.SlotId.HasValue)
            {
                if (!request.SlotId.HasValue)
                    throw new BusinessException("Torba henüz slotta değil — dondurmak için bir slot seçin.");

                targetSlot = await _slotRepository.GetAsync(
                    predicate: s => s.Id == request.SlotId.Value,
                    cancellationToken: cancellationToken
                );
                if (targetSlot is null)
                    throw new BusinessException("Seçilen slot bulunamadı.");
                if (targetSlot.IsOccupied)
                    throw new BusinessException($"Slot ({targetSlot.Position}) dolu; boş bir slot seçin.");

                targetSlot.IsOccupied = true;
                targetSlot.Version += 1;
                bag.SlotId = targetSlot.Id;

                await _slotRepository.UpdateAsync(targetSlot);

                await _movementRepository.AddAsync(new BagMovement
                {
                    BagId = bag.Id,
                    FromSlotId = null,
                    ToSlotId = targetSlot.Id,
                    Action = "Freeze-Store"
                });
            }
            else
            {
                // Zaten slotta — sadece durum değişikliği ve log.
                await _movementRepository.AddAsync(new BagMovement
                {
                    BagId = bag.Id,
                    FromSlotId = bag.SlotId,
                    ToSlotId = bag.SlotId,
                    Action = "Freeze"
                });
            }

            bag.Status = BagStatus.Frozen;
            await _bagRepository.UpdateAsync(bag);

            if (targetSlot is not null)
                await _notifier.BagStoredAsync(bag.Id, targetSlot.Id, cancellationToken);
            await _notifier.DashboardUpdatedAsync(cancellationToken);

            return new FreezeBagResponse
            {
                BagId = bag.Id,
                SlotId = bag.SlotId,
                Status = bag.Status
            };
        }
    }
}

public class FreezeBagResponse
{
    public Guid BagId { get; set; }
    public Guid? SlotId { get; set; }
    public BagStatus Status { get; set; }
}
