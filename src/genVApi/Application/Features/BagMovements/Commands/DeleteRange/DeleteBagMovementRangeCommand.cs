using Application.Features.BagMovements.Constants;
using Application.Features.BagMovements.Rules;
using Application.Services.Repositories;
using Domain.Entities;
using NArchitecture.Core.Application.Pipelines.Authorization;
using NArchitecture.Core.Application.Pipelines.Caching;
using NArchitecture.Core.Application.Pipelines.Logging;
using NArchitecture.Core.Application.Pipelines.Transaction;
using MediatR;
using static Application.Features.BagMovements.Constants.BagMovementsOperationClaims;

namespace Application.Features.BagMovements.Commands.DeleteRange;

public class DeleteBagMovementRangeCommand : IRequest<DeletedBagMovementRangeResponse>, ISecuredRequest, ICacheRemoverRequest, ILoggableRequest, ITransactionalRequest
{
    public ICollection<Guid> Ids { get; set; } = new List<Guid>();

    public string[] Roles => [Admin, Write, BagMovementsOperationClaims.DeleteRange];

    public bool BypassCache { get; }
    public string? CacheKey { get; }
    public string[]? CacheGroupKey => ["GetBagMovements"];

    public class DeleteBagMovementRangeCommandHandler : IRequestHandler<DeleteBagMovementRangeCommand, DeletedBagMovementRangeResponse>
    {
        private readonly IBagMovementRepository _bagMovementRepository;
        private readonly BagMovementBusinessRules _bagMovementBusinessRules;

        public DeleteBagMovementRangeCommandHandler(IBagMovementRepository bagMovementRepository, BagMovementBusinessRules bagMovementBusinessRules)
        {
            _bagMovementRepository = bagMovementRepository;
            _bagMovementBusinessRules = bagMovementBusinessRules;
        }

        public async Task<DeletedBagMovementRangeResponse> Handle(DeleteBagMovementRangeCommand request, CancellationToken cancellationToken)
        {
            List<BagMovement> bagMovements = new List<BagMovement>();

            foreach (Guid id in request.Ids)
            {
                BagMovement? bagMovement = await _bagMovementRepository.GetAsync(
                    predicate: bm => bm.Id == id,
                    cancellationToken: cancellationToken
                );
                await _bagMovementBusinessRules.BagMovementShouldExistWhenSelected(bagMovement);
                bagMovements.Add(bagMovement!);
            }

            await _bagMovementRepository.DeleteRangeAsync(bagMovements);

            return new DeletedBagMovementRangeResponse { DeletedCount = request.Ids.Count };
        }
    }
}
