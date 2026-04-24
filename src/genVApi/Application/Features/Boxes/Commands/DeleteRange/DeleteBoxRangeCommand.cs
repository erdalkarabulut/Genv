using Application.Features.Boxes.Constants;
using Application.Features.Boxes.Rules;
using Application.Services.Repositories;
using Domain.Entities;
using NArchitecture.Core.Application.Pipelines.Authorization;
using NArchitecture.Core.Application.Pipelines.Caching;
using NArchitecture.Core.Application.Pipelines.Logging;
using NArchitecture.Core.Application.Pipelines.Transaction;
using MediatR;
using static Application.Features.Boxes.Constants.BoxesOperationClaims;

namespace Application.Features.Boxes.Commands.DeleteRange;

public class DeleteBoxRangeCommand : IRequest<DeletedBoxRangeResponse>, ISecuredRequest, ICacheRemoverRequest, ILoggableRequest, ITransactionalRequest
{
    public ICollection<Guid> Ids { get; set; } = new List<Guid>();

    public string[] Roles => [Admin, Write, BoxesOperationClaims.DeleteRange];

    public bool BypassCache { get; }
    public string? CacheKey { get; }
    public string[]? CacheGroupKey => ["GetBoxes"];

    public class DeleteBoxRangeCommandHandler : IRequestHandler<DeleteBoxRangeCommand, DeletedBoxRangeResponse>
    {
        private readonly IBoxRepository _boxRepository;
        private readonly BoxBusinessRules _boxBusinessRules;

        public DeleteBoxRangeCommandHandler(IBoxRepository boxRepository, BoxBusinessRules boxBusinessRules)
        {
            _boxRepository = boxRepository;
            _boxBusinessRules = boxBusinessRules;
        }

        public async Task<DeletedBoxRangeResponse> Handle(DeleteBoxRangeCommand request, CancellationToken cancellationToken)
        {
            List<Box> boxs = new List<Box>();

            foreach (Guid id in request.Ids)
            {
                Box? box = await _boxRepository.GetAsync(
                    predicate: b => b.Id == id,
                    cancellationToken: cancellationToken
                );
                await _boxBusinessRules.BoxShouldExistWhenSelected(box);
                boxs.Add(box!);
            }

            await _boxRepository.DeleteRangeAsync(boxs);

            return new DeletedBoxRangeResponse { DeletedCount = request.Ids.Count };
        }
    }
}
