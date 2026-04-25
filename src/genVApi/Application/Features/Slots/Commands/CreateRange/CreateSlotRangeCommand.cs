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
    public string[]? CacheGroupKey => ["GetBagCells"];

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
        private readonly IBagCellRepository _bagCellRepository;

        public CreateSlotRangeCommandHandler(IMapper mapper, IBagCellRepository bagCellRepository)
        {
            _mapper = mapper;
            _bagCellRepository = bagCellRepository;
        }

        public async Task<CreatedSlotRangeResponse> Handle(CreateSlotRangeCommand request, CancellationToken cancellationToken)
        {
            List<BagCell> slots = request.Items.Select(item => _mapper.Map<BagCell>(item)).ToList();

            ICollection<BagCell> addedSlots = await _bagCellRepository.AddRangeAsync(slots);

            return new CreatedSlotRangeResponse { Ids = addedSlots.Select(e => e.Id).ToList() };
        }
    }
}