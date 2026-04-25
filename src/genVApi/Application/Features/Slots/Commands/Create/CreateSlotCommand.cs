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
    public string[]? CacheGroupKey => ["GetBagCells"];

    public class CreateSlotCommandHandler : IRequestHandler<CreateSlotCommand, CreatedSlotResponse>
    {
        private readonly IMapper _mapper;
        private readonly IBagCellRepository _bagCellRepository;
        private readonly BagCellBusinessRules _bagCellBusinessRules;

        public CreateSlotCommandHandler(IMapper mapper, IBagCellRepository bagCellRepository,
                                         BagCellBusinessRules bagCellBusinessRules)
        {
            _mapper = mapper;
            _bagCellRepository = bagCellRepository;
            _bagCellBusinessRules = bagCellBusinessRules;
        }

        public async Task<CreatedSlotResponse> Handle(CreateSlotCommand request, CancellationToken cancellationToken)
        {
            BagCell bagCell = _mapper.Map<BagCell>(request);

            await _bagCellRepository.AddAsync(bagCell);

            CreatedSlotResponse response = _mapper.Map<CreatedSlotResponse>(bagCell);
            return response;
        }
    }
}