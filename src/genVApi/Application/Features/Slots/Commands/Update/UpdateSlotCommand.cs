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
    public string[]? CacheGroupKey => ["GetBagCells"];

    public class UpdateSlotCommandHandler : IRequestHandler<UpdateSlotCommand, UpdatedSlotResponse>
    {
        private readonly IMapper _mapper;
        private readonly IBagCellRepository _bagCellRepository;
        private readonly BagCellBusinessRules _bagCellBusinessRules;

        public UpdateSlotCommandHandler(IMapper mapper, IBagCellRepository bagCellRepository,
                                         BagCellBusinessRules bagCellBusinessRules)
        {
            _mapper = mapper;
            _bagCellRepository = bagCellRepository;
            _bagCellBusinessRules = bagCellBusinessRules;
        }

        public async Task<UpdatedSlotResponse> Handle(UpdateSlotCommand request, CancellationToken cancellationToken)
        {
            BagCell? slot = await _bagCellRepository.GetAsync(predicate: x => x.Id == request.Id, cancellationToken: cancellationToken);
            await _bagCellBusinessRules.BagCellShouldExistWhenSelected(slot);
            slot = _mapper.Map(request, slot);

            await _bagCellRepository.UpdateAsync(slot!);

            UpdatedSlotResponse response = _mapper.Map<UpdatedSlotResponse>(slot);
            return response;
        }
    }
}