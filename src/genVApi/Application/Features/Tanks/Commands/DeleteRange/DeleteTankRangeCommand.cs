using Application.Features.Tanks.Constants;
using Application.Features.Tanks.Rules;
using Application.Services.Repositories;
using Domain.Entities;
using NArchitecture.Core.Application.Pipelines.Authorization;
using NArchitecture.Core.Application.Pipelines.Caching;
using NArchitecture.Core.Application.Pipelines.Logging;
using NArchitecture.Core.Application.Pipelines.Transaction;
using MediatR;
using static Application.Features.Tanks.Constants.TanksOperationClaims;

namespace Application.Features.Tanks.Commands.DeleteRange;

public class DeleteTankRangeCommand : IRequest<DeletedTankRangeResponse>, ISecuredRequest, ICacheRemoverRequest, ILoggableRequest, ITransactionalRequest
{
    public ICollection<Guid> Ids { get; set; } = new List<Guid>();

    public string[] Roles => [Admin, Write, TanksOperationClaims.DeleteRange];

    public bool BypassCache { get; }
    public string? CacheKey { get; }
    public string[]? CacheGroupKey => ["GetTanks"];

    public class DeleteTankRangeCommandHandler : IRequestHandler<DeleteTankRangeCommand, DeletedTankRangeResponse>
    {
        private readonly ITankRepository _tankRepository;
        private readonly TankBusinessRules _tankBusinessRules;

        public DeleteTankRangeCommandHandler(ITankRepository tankRepository, TankBusinessRules tankBusinessRules)
        {
            _tankRepository = tankRepository;
            _tankBusinessRules = tankBusinessRules;
        }

        public async Task<DeletedTankRangeResponse> Handle(DeleteTankRangeCommand request, CancellationToken cancellationToken)
        {
            List<Tank> tanks = new List<Tank>();

            foreach (Guid id in request.Ids)
            {
                Tank? tank = await _tankRepository.GetAsync(
                    predicate: t => t.Id == id,
                    cancellationToken: cancellationToken
                );
                await _tankBusinessRules.TankShouldExistWhenSelected(tank);
                tanks.Add(tank!);
            }

            await _tankRepository.DeleteRangeAsync(tanks);

            return new DeletedTankRangeResponse { DeletedCount = request.Ids.Count };
        }
    }
}
