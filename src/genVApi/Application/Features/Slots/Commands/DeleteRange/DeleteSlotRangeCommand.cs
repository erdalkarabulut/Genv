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
    public string[]? CacheGroupKey => ["GetSlots"];

    public class DeleteSlotRangeCommandHandler : IRequestHandler<DeleteSlotRangeCommand, DeletedSlotRangeResponse>
    {
        private readonly ISlotRepository _slotRepository;
        private readonly SlotBusinessRules _slotBusinessRules;

        public DeleteSlotRangeCommandHandler(ISlotRepository slotRepository, SlotBusinessRules slotBusinessRules)
        {
            _slotRepository = slotRepository;
            _slotBusinessRules = slotBusinessRules;
        }

        public async Task<DeletedSlotRangeResponse> Handle(DeleteSlotRangeCommand request, CancellationToken cancellationToken)
        {
            List<Slot> slots = new List<Slot>();

            foreach (Guid id in request.Ids)
            {
                Slot? slot = await _slotRepository.GetAsync(
                    predicate: s => s.Id == id,
                    cancellationToken: cancellationToken
                );
                await _slotBusinessRules.SlotShouldExistWhenSelected(slot);
                slots.Add(slot!);
            }

            await _slotRepository.DeleteRangeAsync(slots);

            return new DeletedSlotRangeResponse { DeletedCount = request.Ids.Count };
        }
    }
}
