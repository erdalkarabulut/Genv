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
/// Opsiyonel: BagCellId verilirse torba önce o torba hücresine yerleştirilir, sonra Frozen yapılır.
/// </summary>
public class FreezeBagCommand : IRequest<FreezeBagResponse>, ISecuredRequest, ICacheRemoverRequest, ILoggableRequest, ITransactionalRequest
{
    public Guid BagId { get; set; }

    /// <summary>Torba henüz bir hücrede değilse verilmeli. Zaten hücrede ise boş bırakılır.</summary>
    public Guid? BagCellId { get; set; }

    public string[] Roles => [Admin, Write];

    public bool BypassCache { get; }
    public string? CacheKey { get; }
    public string[]? CacheGroupKey => ["GetBags", "GetBagCells", "Dashboard"];

    public class FreezeBagCommandHandler : IRequestHandler<FreezeBagCommand, FreezeBagResponse>
    {
        private readonly IBagRepository _bagRepository;
        private readonly IBagCellRepository _bagCellRepository;
        private readonly IBagMovementRepository _movementRepository;
        private readonly BagBusinessRules _bagBusinessRules;
        private readonly IRealTimeNotifier _notifier;

        public FreezeBagCommandHandler(
            IBagRepository bagRepository,
            IBagCellRepository bagCellRepository,
            IBagMovementRepository movementRepository,
            BagBusinessRules bagBusinessRules,
            IRealTimeNotifier notifier)
        {
            _bagRepository = bagRepository;
            _bagCellRepository = bagCellRepository;
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

            BagCell? targetCell = null;
            if (!bag.BagCellId.HasValue)
            {
                if (!request.BagCellId.HasValue)
                    throw new BusinessException("Torba henüz bir torba hücresinde değil — dondurmak için bir hücre seçin.");

                targetCell = await _bagCellRepository.GetAsync(
                    predicate: c => c.Id == request.BagCellId.Value,
                    cancellationToken: cancellationToken
                );
                if (targetCell is null)
                    throw new BusinessException("Seçilen torba hücresi bulunamadı.");
                if (targetCell.IsOccupied)
                    throw new BusinessException($"Torba hücresi ({targetCell.Position}) dolu; boş bir hücre seçin.");

                targetCell.IsOccupied = true;
                targetCell.Version += 1;
                bag.BagCellId = targetCell.Id;

                await _bagCellRepository.UpdateAsync(targetCell);

                await _movementRepository.AddAsync(new BagMovement
                {
                    BagId = bag.Id,
                    FromBagCellId = null,
                    ToBagCellId = targetCell.Id,
                    Action = "Freeze-Store"
                });
            }
            else
            {
                await _movementRepository.AddAsync(new BagMovement
                {
                    BagId = bag.Id,
                    FromBagCellId = bag.BagCellId,
                    ToBagCellId = bag.BagCellId,
                    Action = "Freeze"
                });
            }

            bag.Status = BagStatus.Frozen;
            await _bagRepository.UpdateAsync(bag);

            if (targetCell is not null)
                await _notifier.BagStoredAsync(bag.Id, targetCell.Id, cancellationToken);
            await _notifier.DashboardUpdatedAsync(cancellationToken);

            return new FreezeBagResponse
            {
                BagId = bag.Id,
                BagCellId = bag.BagCellId,
                Status = bag.Status
            };
        }
    }
}

public class FreezeBagResponse
{
    public Guid BagId { get; set; }
    public Guid? BagCellId { get; set; }
    public BagStatus Status { get; set; }
}
