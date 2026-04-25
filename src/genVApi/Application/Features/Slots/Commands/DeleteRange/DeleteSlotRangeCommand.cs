using Application.Features.Slots.Constants;
using Application.Features.Slots.Rules;
using Application.Services.Repositories;
using Domain.Entities;
using NArchitecture.Core.Application.Pipelines.Authorization;
using NArchitecture.Core.Application.Pipelines.Caching;
using NArchitecture.Core.Application.Pipelines.Logging;
using NArchitecture.Core.Application.Pipelines.Transaction;
using MediatR;
using static Application.Features.Slots.Constants.SlotsOperationClaims;

namespace Application.Features.Slots.Commands.DeleteRange;

public class DeleteSlotRangeCommand : IRequest<DeletedSlotRangeResponse>, ISecuredRequest, ICacheRemoverRequest, ILoggableRequest, ITransactionalRequest
{
    public ICollection<Guid> Ids { get; set; } = new List<Guid>();

    public string[] Roles => [Admin, Write, SlotsOperationClaims.DeleteRange];

    public bool BypassCache { get; }
    public string? CacheKey { get; }
    public string[]? CacheGroupKey => ["GetBagCells"];

    public class DeleteSlotRangeCommandHandler : IRequestHandler<DeleteSlotRangeCommand, DeletedSlotRangeResponse>
    {
        private readonly IBagCellRepository _bagCellRepository;
        private readonly BagCellBusinessRules _bagCellBusinessRules;

        public DeleteSlotRangeCommandHandler(IBagCellRepository bagCellRepository, BagCellBusinessRules bagCellBusinessRules)
        {
            _bagCellRepository = bagCellRepository;
            _bagCellBusinessRules = bagCellBusinessRules;
        }

        public async Task<DeletedSlotRangeResponse> Handle(DeleteSlotRangeCommand request, CancellationToken cancellationToken)
        {
            List<BagCell> slots = new List<BagCell>();

            foreach (Guid id in request.Ids)
            {
                BagCell? slot = await _bagCellRepository.GetAsync(
                    predicate: s => s.Id == id,
                    cancellationToken: cancellationToken
                );
                await _bagCellBusinessRules.BagCellShouldExistWhenSelected(slot);
                slots.Add(slot!);
            }

            await _bagCellRepository.DeleteRangeAsync(slots);

            return new DeletedSlotRangeResponse { DeletedCount = request.Ids.Count };
        }
    }
}
