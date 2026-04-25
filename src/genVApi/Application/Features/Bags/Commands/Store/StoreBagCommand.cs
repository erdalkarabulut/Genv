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
    public Guid BagCellId { get; set; }

    public string[] Roles => [Admin, Write];

    public bool BypassCache { get; }
    public string? CacheKey { get; }
    public string[]? CacheGroupKey => ["GetBags", "GetBagCells", "Dashboard"];

    public class StoreBagCommandHandler : IRequestHandler<StoreBagCommand, StoreBagResponse>
    {
        private readonly IBagRepository _bagRepository;
        private readonly IBagCellRepository _bagCellRepository;
        private readonly IBagMovementRepository _movementRepository;
        private readonly BagBusinessRules _bagBusinessRules;
        private readonly IRealTimeNotifier _notifier;

        public StoreBagCommandHandler(
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

        public async Task<StoreBagResponse> Handle(StoreBagCommand request, CancellationToken cancellationToken)
        {
            Bag? bag = await _bagRepository.GetAsync(predicate: b => b.Id == request.BagId, cancellationToken: cancellationToken);
            await _bagBusinessRules.BagShouldExistWhenSelected(bag);

            if (bag!.BagCellId.HasValue)
                throw new BusinessException("Bag is already stored in a bag cell. Move it instead.");

            BagCell? cell = await _bagCellRepository.GetAsync(predicate: c => c.Id == request.BagCellId, cancellationToken: cancellationToken);
            if (cell is null)
                throw new BusinessException("Bag cell not found.");
            if (cell.IsOccupied)
                throw new BusinessException("Bag cell is already occupied.");

            cell.IsOccupied = true;
            cell.Version += 1;
            bag.BagCellId = cell.Id;
            bag.Status = BagStatus.Stored;

            await _bagCellRepository.UpdateAsync(cell);
            await _bagRepository.UpdateAsync(bag);
            await _movementRepository.AddAsync(new BagMovement
            {
                BagId = bag.Id,
                FromBagCellId = null,
                ToBagCellId = cell.Id,
                Action = "Store"
            });

            await _notifier.BagStoredAsync(bag.Id, cell.Id, cancellationToken);
            await _notifier.DashboardUpdatedAsync(cancellationToken);

            return new StoreBagResponse { BagId = bag.Id, BagCellId = cell.Id, Status = bag.Status };
        }
    }
}

public class StoreBagResponse
{
    public Guid BagId { get; set; }
    public Guid BagCellId { get; set; }
    public BagStatus Status { get; set; }
}
