using Application.Features.Racks.Constants;
using Application.Features.Racks.Rules;
using Application.Services.Repositories;
using Domain.Entities;
using NArchitecture.Core.Application.Pipelines.Authorization;
using NArchitecture.Core.Application.Pipelines.Caching;
using NArchitecture.Core.Application.Pipelines.Logging;
using NArchitecture.Core.Application.Pipelines.Transaction;
using MediatR;
using static Application.Features.Racks.Constants.RacksOperationClaims;

namespace Application.Features.Racks.Commands.DeleteRange;

public class DeleteRackRangeCommand : IRequest<DeletedRackRangeResponse>, ISecuredRequest, ICacheRemoverRequest, ILoggableRequest, ITransactionalRequest
{
    public ICollection<Guid> Ids { get; set; } = new List<Guid>();

    public string[] Roles => [Admin, Write, RacksOperationClaims.DeleteRange];

    public bool BypassCache { get; }
    public string? CacheKey { get; }
    public string[]? CacheGroupKey => ["GetRacks"];

    public class DeleteRackRangeCommandHandler : IRequestHandler<DeleteRackRangeCommand, DeletedRackRangeResponse>
    {
        private readonly IRackRepository _rackRepository;
        private readonly RackBusinessRules _rackBusinessRules;

        public DeleteRackRangeCommandHandler(IRackRepository rackRepository, RackBusinessRules rackBusinessRules)
        {
            _rackRepository = rackRepository;
            _rackBusinessRules = rackBusinessRules;
        }

        public async Task<DeletedRackRangeResponse> Handle(DeleteRackRangeCommand request, CancellationToken cancellationToken)
        {
            List<Rack> racks = new List<Rack>();

            foreach (Guid id in request.Ids)
            {
                Rack? rack = await _rackRepository.GetAsync(
                    predicate: r => r.Id == id,
                    cancellationToken: cancellationToken
                );
                await _rackBusinessRules.RackShouldExistWhenSelected(rack);
                racks.Add(rack!);
            }

            await _rackRepository.DeleteRangeAsync(racks);

            return new DeletedRackRangeResponse { DeletedCount = request.Ids.Count };
        }
    }
}
