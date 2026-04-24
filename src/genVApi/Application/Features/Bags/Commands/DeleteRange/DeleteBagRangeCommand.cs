using Application.Features.Bags.Constants;
using Application.Features.Bags.Rules;
using Application.Services.Repositories;
using Domain.Entities;
using NArchitecture.Core.Application.Pipelines.Authorization;
using NArchitecture.Core.Application.Pipelines.Caching;
using NArchitecture.Core.Application.Pipelines.Logging;
using NArchitecture.Core.Application.Pipelines.Transaction;
using MediatR;
using static Application.Features.Bags.Constants.BagsOperationClaims;

namespace Application.Features.Bags.Commands.DeleteRange;

public class DeleteBagRangeCommand : IRequest<DeletedBagRangeResponse>, ISecuredRequest, ICacheRemoverRequest, ILoggableRequest, ITransactionalRequest
{
    public ICollection<Guid> Ids { get; set; } = new List<Guid>();

    public string[] Roles => [Admin, Write, BagsOperationClaims.DeleteRange];

    public bool BypassCache { get; }
    public string? CacheKey { get; }
    public string[]? CacheGroupKey => ["GetBags"];

    public class DeleteBagRangeCommandHandler : IRequestHandler<DeleteBagRangeCommand, DeletedBagRangeResponse>
    {
        private readonly IBagRepository _bagRepository;
        private readonly BagBusinessRules _bagBusinessRules;

        public DeleteBagRangeCommandHandler(IBagRepository bagRepository, BagBusinessRules bagBusinessRules)
        {
            _bagRepository = bagRepository;
            _bagBusinessRules = bagBusinessRules;
        }

        public async Task<DeletedBagRangeResponse> Handle(DeleteBagRangeCommand request, CancellationToken cancellationToken)
        {
            List<Bag> bags = new List<Bag>();

            foreach (Guid id in request.Ids)
            {
                Bag? bag = await _bagRepository.GetAsync(
                    predicate: b => b.Id == id,
                    cancellationToken: cancellationToken
                );
                await _bagBusinessRules.BagShouldExistWhenSelected(bag);
                bags.Add(bag!);
            }

            await _bagRepository.DeleteRangeAsync(bags);

            return new DeletedBagRangeResponse { DeletedCount = request.Ids.Count };
        }
    }
}
