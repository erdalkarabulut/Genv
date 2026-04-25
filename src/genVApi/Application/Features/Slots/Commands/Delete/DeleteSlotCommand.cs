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
    public string[]? CacheGroupKey => ["GetBagCells"];

    public class DeleteSlotCommandHandler : IRequestHandler<DeleteSlotCommand, DeletedSlotResponse>
    {
        private readonly IMapper _mapper;
        private readonly IBagCellRepository _bagCellRepository;
        private readonly BagCellBusinessRules _bagCellBusinessRules;

        public DeleteSlotCommandHandler(IMapper mapper, IBagCellRepository bagCellRepository,
                                         BagCellBusinessRules bagCellBusinessRules)
        {
            _mapper = mapper;
            _bagCellRepository = bagCellRepository;
            _bagCellBusinessRules = bagCellBusinessRules;
        }

        public async Task<DeletedSlotResponse> Handle(DeleteSlotCommand request, CancellationToken cancellationToken)
        {
            BagCell? slot = await _bagCellRepository.GetAsync(predicate: s => s.Id == request.Id, cancellationToken: cancellationToken);
            await _bagCellBusinessRules.BagCellShouldExistWhenSelected(slot);

            await _bagCellRepository.DeleteAsync(slot!);

            DeletedSlotResponse response = _mapper.Map<DeletedSlotResponse>(slot);
            return response;
        }
    }
}