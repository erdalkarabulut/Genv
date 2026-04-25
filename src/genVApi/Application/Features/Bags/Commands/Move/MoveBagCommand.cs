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
    /// <summary>Hedef torba hücresi (BagCell) id.</summary>
    public Guid TargetBagCellId { get; set; }

    public string[] Roles => [Admin, Write];

    public bool BypassCache { get; }
    public string? CacheKey { get; }
    public string[]? CacheGroupKey => ["GetBags", "GetBagCells", "Dashboard"];

    public class MoveBagCommandHandler : IRequestHandler<MoveBagCommand, MoveBagResponse>
    {
        private readonly IBagRepository _bagRepository;
        private readonly IBagCellRepository _bagCellRepository;
        private readonly IBagMovementRepository _movementRepository;
        private readonly BagBusinessRules _bagBusinessRules;
        private readonly IRealTimeNotifier _notifier;

        public MoveBagCommandHandler(
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

        public async Task<MoveBagResponse> Handle(MoveBagCommand request, CancellationToken cancellationToken)
        {
            Bag? bag = await _bagRepository.GetAsync(predicate: b => b.Id == request.BagId, cancellationToken: cancellationToken);
            await _bagBusinessRules.BagShouldExistWhenSelected(bag);

            if (!bag!.BagCellId.HasValue)
                throw new BusinessException("Bag is not stored in any bag cell. Store it first.");

            Guid fromCellId = bag.BagCellId.Value;
            if (fromCellId == request.TargetBagCellId)
                return new MoveBagResponse { BagId = bag.Id, FromBagCellId = fromCellId, ToBagCellId = fromCellId };

            BagCell? fromCell = await _bagCellRepository.GetAsync(predicate: c => c.Id == fromCellId, cancellationToken: cancellationToken);
            BagCell? toCell = await _bagCellRepository.GetAsync(predicate: c => c.Id == request.TargetBagCellId, cancellationToken: cancellationToken);
            if (fromCell is null || toCell is null)
                throw new BusinessException("Source or target bag cell not found.");
            if (toCell.IsOccupied)
                throw new BusinessException("Target bag cell is occupied.");

            fromCell.IsOccupied = false;
            fromCell.Version += 1;
            toCell.IsOccupied = true;
            toCell.Version += 1;
            bag.BagCellId = toCell.Id;

            await _bagCellRepository.UpdateAsync(fromCell);
            await _bagCellRepository.UpdateAsync(toCell);
            await _bagRepository.UpdateAsync(bag);
            await _movementRepository.AddAsync(new BagMovement
            {
                BagId = bag.Id,
                FromBagCellId = fromCell.Id,
                ToBagCellId = toCell.Id,
                Action = "Move"
            });

            await _notifier.BagMovedAsync(bag.Id, fromCell.Id, toCell.Id, cancellationToken);
            await _notifier.DashboardUpdatedAsync(cancellationToken);

            return new MoveBagResponse { BagId = bag.Id, FromBagCellId = fromCell.Id, ToBagCellId = toCell.Id };
        }
    }
}

public class MoveBagResponse
{
    public Guid BagId { get; set; }
    public Guid FromBagCellId { get; set; }
    public Guid ToBagCellId { get; set; }
}
