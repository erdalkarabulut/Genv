using Application.Features.Slots.Constants;
using Application.Services.Repositories;
using AutoMapper;
using Domain.Entities;
using MediatR;
using NArchitecture.Core.Application.Pipelines.Authorization;
using NArchitecture.Core.Application.Pipelines.Caching;
using NArchitecture.Core.Application.Pipelines.Logging;
using NArchitecture.Core.Application.Pipelines.Transaction;
using static Application.Features.Slots.Constants.SlotsOperationClaims;

namespace Application.Features.Slots.Commands.CreateRange;

public class CreateSlotRangeCommand : IRequest<CreatedSlotRangeResponse>, ISecuredRequest, ICacheRemoverRequest, ILoggableRequest, ITransactionalRequest
{
    public ICollection<CreateSlotRangeItem> Items { get; set; } = new List<CreateSlotRangeItem>();

    public string[] Roles => [Admin, Write, SlotsOperationClaims.CreateRange];

    public bool BypassCache { get; }
    public string? CacheKey { get; }
    public string[]? CacheGroupKey => ["GetSlots"];

    public class CreateSlotRangeItem
    {
        public Guid BoxId { get; set; }
        public string Position { get; set; }
        public bool IsOccupied { get; set; }
        public int Version { get; set; }
    }

    public class CreateSlotRangeCommandHandler : IRequestHandler<CreateSlotRangeCommand, CreatedSlotRangeResponse>
    {
        private readonly IMapper _mapper;
        private readonly ISlotRepository _slotRepository;

        public CreateSlotRangeCommandHandler(IMapper mapper, ISlotRepository slotRepository)
        {
            _mapper = mapper;
            _slotRepository = slotRepository;
        }

        public async Task<CreatedSlotRangeResponse> Handle(CreateSlotRangeCommand request, CancellationToken cancellationToken)
        {
            List<Slot> slots = request.Items.Select(item => _mapper.Map<Slot>(item)).ToList();

            ICollection<Slot> addedSlots = await _slotRepository.AddRangeAsync(slots);

            return new CreatedSlotRangeResponse { Ids = addedSlots.Select(e => e.Id).ToList() };
        }
    }
}