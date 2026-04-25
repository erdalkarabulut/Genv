using Application.Features.BagMovements.Constants;
using Application.Features.BagMovements.Rules;
using Application.Services.Repositories;
using AutoMapper;
using Domain.Entities;
using MediatR;
using NArchitecture.Core.Application.Pipelines.Authorization;
using NArchitecture.Core.Application.Pipelines.Caching;
using NArchitecture.Core.Application.Pipelines.Logging;
using NArchitecture.Core.Application.Pipelines.Transaction;
using static Application.Features.BagMovements.Constants.BagMovementsOperationClaims;

namespace Application.Features.BagMovements.Commands.UpdateRange;

public class UpdateBagMovementRangeCommand : IRequest<UpdatedBagMovementRangeResponse>, ISecuredRequest, ICacheRemoverRequest, ILoggableRequest, ITransactionalRequest
{
    public ICollection<UpdateBagMovementRangeItem> Items { get; set; } = new List<UpdateBagMovementRangeItem>();

    public string[] Roles => [Admin, Write, BagMovementsOperationClaims.UpdateRange];

    public bool BypassCache { get; }
    public string? CacheKey { get; }
    public string[]? CacheGroupKey => ["GetBagMovements"];

    public class UpdateBagMovementRangeItem
    {
        public Guid Id { get; set; }
        public Guid BagId { get; set; }
        public Guid? FromBagCellId { get; set; }
        public Guid? ToBagCellId { get; set; }
        public string Action { get; set; }
    }

    public class UpdateBagMovementRangeCommandHandler : IRequestHandler<UpdateBagMovementRangeCommand, UpdatedBagMovementRangeResponse>
    {
        private readonly IMapper _mapper;
        private readonly IBagMovementRepository _bagMovementRepository;
        private readonly BagMovementBusinessRules _bagMovementBusinessRules;

        public UpdateBagMovementRangeCommandHandler(IMapper mapper, IBagMovementRepository bagMovementRepository, BagMovementBusinessRules bagMovementBusinessRules)
        {
            _mapper = mapper;
            _bagMovementRepository = bagMovementRepository;
            _bagMovementBusinessRules = bagMovementBusinessRules;
        }

        public async Task<UpdatedBagMovementRangeResponse> Handle(UpdateBagMovementRangeCommand request, CancellationToken cancellationToken)
        {
            List<BagMovement> items = new List<BagMovement>();

            foreach (UpdateBagMovementRangeItem item in request.Items)
            {
                BagMovement? entity = await _bagMovementRepository.GetAsync(
                    predicate: x => x.Id == item.Id,
                    cancellationToken: cancellationToken
                );
                await _bagMovementBusinessRules.BagMovementShouldExistWhenSelected(entity);
                _mapper.Map(source: item, destination: entity!);
                items.Add(entity!);
            }

            ICollection<BagMovement> updated = await _bagMovementRepository.UpdateRangeAsync(items);

            return new UpdatedBagMovementRangeResponse { Ids = updated.Select(e => e.Id).ToList() };
        }
    }
}