using Application.Features.Slots.Constants;
using Application.Features.Slots.Rules;
using Application.Services.Repositories;
using AutoMapper;
using Domain.Entities;
using NArchitecture.Core.Application.Pipelines.Authorization;
using NArchitecture.Core.Application.Pipelines.Caching;
using NArchitecture.Core.Application.Pipelines.Logging;
using NArchitecture.Core.Application.Pipelines.Transaction;
using MediatR;
using static Application.Features.Slots.Constants.SlotsOperationClaims;

namespace Application.Features.Slots.Commands.Update;

public class UpdateSlotCommand : IRequest<UpdatedSlotResponse>, ISecuredRequest, ICacheRemoverRequest, ILoggableRequest, ITransactionalRequest
{
    public Guid Id { get; set; }
    public Guid BoxId { get; set; }
    public string Position { get; set; }
    public bool IsOccupied { get; set; }
    public int Version { get; set; }

    public string[] Roles => [Admin, Write, SlotsOperationClaims.Update];

    public bool BypassCache { get; }
    public string? CacheKey { get; }
    public string[]? CacheGroupKey => ["GetSlots"];

    public class UpdateSlotCommandHandler : IRequestHandler<UpdateSlotCommand, UpdatedSlotResponse>
    {
        private readonly IMapper _mapper;
        private readonly ISlotRepository _slotRepository;
        private readonly SlotBusinessRules _slotBusinessRules;

        public UpdateSlotCommandHandler(IMapper mapper, ISlotRepository slotRepository,
                                         SlotBusinessRules slotBusinessRules)
        {
            _mapper = mapper;
            _slotRepository = slotRepository;
            _slotBusinessRules = slotBusinessRules;
        }

        public async Task<UpdatedSlotResponse> Handle(UpdateSlotCommand request, CancellationToken cancellationToken)
        {
            Slot? slot = await _slotRepository.GetAsync(predicate: x => x.Id == request.Id, cancellationToken: cancellationToken);
            await _slotBusinessRules.SlotShouldExistWhenSelected(slot);
            slot = _mapper.Map(request, slot);

            await _slotRepository.UpdateAsync(slot!);

            UpdatedSlotResponse response = _mapper.Map<UpdatedSlotResponse>(slot);
            return response;
        }
    }
}