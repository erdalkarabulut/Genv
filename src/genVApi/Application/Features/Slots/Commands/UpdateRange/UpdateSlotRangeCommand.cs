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
    public string[]? CacheGroupKey => ["GetSlots"];

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
        private readonly ISlotRepository _slotRepository;
        private readonly SlotBusinessRules _slotBusinessRules;

        public UpdateSlotRangeCommandHandler(IMapper mapper, ISlotRepository slotRepository, SlotBusinessRules slotBusinessRules)
        {
            _mapper = mapper;
            _slotRepository = slotRepository;
            _slotBusinessRules = slotBusinessRules;
        }

        public async Task<UpdatedSlotRangeResponse> Handle(UpdateSlotRangeCommand request, CancellationToken cancellationToken)
        {
            List<Slot> items = new List<Slot>();

            foreach (UpdateSlotRangeItem item in request.Items)
            {
                Slot? entity = await _slotRepository.GetAsync(
                    predicate: x => x.Id == item.Id,
                    cancellationToken: cancellationToken
                );
                await _slotBusinessRules.SlotShouldExistWhenSelected(entity);
                _mapper.Map(source: item, destination: entity!);
                items.Add(entity!);
            }

            ICollection<Slot> updated = await _slotRepository.UpdateRangeAsync(items);

            return new UpdatedSlotRangeResponse { Ids = updated.Select(e => e.Id).ToList() };
        }
    }
}