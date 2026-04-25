using Application.Features.Slots.Constants;
using Application.Features.Slots.Rules;
using Application.Services.Repositories;
using AutoMapper;
using Domain.Entities;
using MediatR;
using NArchitecture.Core.Application.Pipelines.Authorization;
using NArchitecture.Core.Application.Pipelines.Caching;
using NArchitecture.Core.Application.Pipelines.Logging;
using NArchitecture.Core.Application.Pipelines.Transaction;
using static Application.Features.Slots.Constants.SlotsOperationClaims;

namespace Application.Features.Slots.Commands.UpdateRange;

public class UpdateSlotRangeCommand : IRequest<UpdatedSlotRangeResponse>, ISecuredRequest, ICacheRemoverRequest, ILoggableRequest, ITransactionalRequest
{
    public ICollection<UpdateSlotRangeItem> Items { get; set; } = new List<UpdateSlotRangeItem>();

    public string[] Roles => [Admin, Write, SlotsOperationClaims.UpdateRange];

    public bool BypassCache { get; }
    public string? CacheKey { get; }
    public string[]? CacheGroupKey => ["GetBagCells"];

    public class UpdateSlotRangeItem
    {
        public Guid Id { get; set; }
        public Guid BoxId { get; set; }
        public string Position { get; set; }
        public bool IsOccupied { get; set; }
        public int Version { get; set; }
    }

    public class UpdateSlotRangeCommandHandler : IRequestHandler<UpdateSlotRangeCommand, UpdatedSlotRangeResponse>
    {
        private readonly IMapper _mapper;
        private readonly IBagCellRepository _bagCellRepository;
        private readonly BagCellBusinessRules _bagCellBusinessRules;

        public UpdateSlotRangeCommandHandler(IMapper mapper, IBagCellRepository bagCellRepository, BagCellBusinessRules bagCellBusinessRules)
        {
            _mapper = mapper;
            _bagCellRepository = bagCellRepository;
            _bagCellBusinessRules = bagCellBusinessRules;
        }

        public async Task<UpdatedSlotRangeResponse> Handle(UpdateSlotRangeCommand request, CancellationToken cancellationToken)
        {
            List<BagCell> items = new List<BagCell>();

            foreach (UpdateSlotRangeItem item in request.Items)
            {
                BagCell? entity = await _bagCellRepository.GetAsync(
                    predicate: x => x.Id == item.Id,
                    cancellationToken: cancellationToken
                );
                await _bagCellBusinessRules.BagCellShouldExistWhenSelected(entity);
                _mapper.Map(source: item, destination: entity!);
                items.Add(entity!);
            }

            ICollection<BagCell> updated = await _bagCellRepository.UpdateRangeAsync(items);

            return new UpdatedSlotRangeResponse { Ids = updated.Select(e => e.Id).ToList() };
        }
    }
}