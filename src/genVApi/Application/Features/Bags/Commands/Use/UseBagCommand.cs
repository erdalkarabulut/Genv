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

    /// <summary>Kullanım sebebi. Other seçilirse Note zorunludur.</summary>
    public BagUseReason Reason { get; set; } = BagUseReason.Infusion;

    /// <summary>Serbest not. Reason == Other ise istemcide zorunlu.</summary>
    public string? Note { get; set; }

    public string[] Roles => [Admin, Write];

    public bool BypassCache { get; }
    public string? CacheKey { get; }
    public string[]? CacheGroupKey => ["GetBags", "GetBagCells", "Dashboard"];

    public class UseBagCommandHandler : IRequestHandler<UseBagCommand, UseBagResponse>
    {
        private readonly IBagRepository _bagRepository;
        private readonly IBagCellRepository _bagCellRepository;
        private readonly IBagMovementRepository _movementRepository;
        private readonly BagBusinessRules _bagBusinessRules;
        private readonly IRealTimeNotifier _notifier;

        public UseBagCommandHandler(
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

        public async Task<UseBagResponse> Handle(UseBagCommand request, CancellationToken cancellationToken)
        {
            Bag? bag = await _bagRepository.GetAsync(predicate: b => b.Id == request.BagId, cancellationToken: cancellationToken);
            await _bagBusinessRules.BagShouldExistWhenSelected(bag);

            if (bag!.Status == BagStatus.Used)
                throw new BusinessException("Bag is already used.");

            Guid? fromCellId = bag.BagCellId;
            if (fromCellId.HasValue)
            {
                BagCell? cell = await _bagCellRepository.GetAsync(predicate: c => c.Id == fromCellId.Value, cancellationToken: cancellationToken);
                if (cell != null)
                {
                    cell.IsOccupied = false;
                    cell.Version += 1;
                    await _bagCellRepository.UpdateAsync(cell);
                }
                bag.BagCellId = null;
            }

            bag.Status = BagStatus.Used;
            bag.UseReason = request.Reason;
            bag.UseNote = string.IsNullOrWhiteSpace(request.Note) ? null : request.Note!.Trim();
            await _bagRepository.UpdateAsync(bag);

            await _movementRepository.AddAsync(new BagMovement
            {
                BagId = bag.Id,
                FromBagCellId = fromCellId,
                ToBagCellId = null,
                Action = $"Use:{request.Reason}",
                UsedAt = DateTime.UtcNow
            });

            await _notifier.BagUsedAsync(bag.Id, fromCellId, cancellationToken);
            await _notifier.DashboardUpdatedAsync(cancellationToken);

            return new UseBagResponse { BagId = bag.Id, FreedBagCellId = fromCellId };
        }
    }
}

public class UseBagResponse
{
    public Guid BagId { get; set; }
    public Guid? FreedBagCellId { get; set; }
}
