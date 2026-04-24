using Application.Features.Slots.Constants;
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

namespace Application.Features.Slots.Commands.Delete;

public class DeleteSlotCommand : IRequest<DeletedSlotResponse>, ISecuredRequest, ICacheRemoverRequest, ILoggableRequest, ITransactionalRequest
{
    public Guid Id { get; set; }

    public string[] Roles => [Admin, Write, SlotsOperationClaims.Delete];

    public bool BypassCache { get; }
    public string? CacheKey { get; }
    public string[]? CacheGroupKey => ["GetSlots"];

    public class DeleteSlotCommandHandler : IRequestHandler<DeleteSlotCommand, DeletedSlotResponse>
    {
        private readonly IMapper _mapper;
        private readonly ISlotRepository _slotRepository;
        private readonly SlotBusinessRules _slotBusinessRules;

        public DeleteSlotCommandHandler(IMapper mapper, ISlotRepository slotRepository,
                                         SlotBusinessRules slotBusinessRules)
        {
            _mapper = mapper;
            _slotRepository = slotRepository;
            _slotBusinessRules = slotBusinessRules;
        }

        public async Task<DeletedSlotResponse> Handle(DeleteSlotCommand request, CancellationToken cancellationToken)
        {
            Slot? slot = await _slotRepository.GetAsync(predicate: s => s.Id == request.Id, cancellationToken: cancellationToken);
            await _slotBusinessRules.SlotShouldExistWhenSelected(slot);

            await _slotRepository.DeleteAsync(slot!);

            DeletedSlotResponse response = _mapper.Map<DeletedSlotResponse>(slot);
            return response;
        }
    }
}