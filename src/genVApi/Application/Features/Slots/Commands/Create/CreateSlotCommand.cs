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

namespace Application.Features.Slots.Commands.Create;

public class CreateSlotCommand : IRequest<CreatedSlotResponse>, ISecuredRequest, ICacheRemoverRequest, ILoggableRequest, ITransactionalRequest
{
    public Guid BoxId { get; set; }
    public string Position { get; set; }
    public bool IsOccupied { get; set; }
    public int Version { get; set; }

    public string[] Roles => [Admin, Write, SlotsOperationClaims.Create];

    public bool BypassCache { get; }
    public string? CacheKey { get; }
    public string[]? CacheGroupKey => ["GetSlots"];

    public class CreateSlotCommandHandler : IRequestHandler<CreateSlotCommand, CreatedSlotResponse>
    {
        private readonly IMapper _mapper;
        private readonly ISlotRepository _slotRepository;
        private readonly SlotBusinessRules _slotBusinessRules;

        public CreateSlotCommandHandler(IMapper mapper, ISlotRepository slotRepository,
                                         SlotBusinessRules slotBusinessRules)
        {
            _mapper = mapper;
            _slotRepository = slotRepository;
            _slotBusinessRules = slotBusinessRules;
        }

        public async Task<CreatedSlotResponse> Handle(CreateSlotCommand request, CancellationToken cancellationToken)
        {
            Slot slot = _mapper.Map<Slot>(request);

            await _slotRepository.AddAsync(slot);

            CreatedSlotResponse response = _mapper.Map<CreatedSlotResponse>(slot);
            return response;
        }
    }
}