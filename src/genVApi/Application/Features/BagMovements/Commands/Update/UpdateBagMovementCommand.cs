using Application.Features.BagMovements.Constants;
using Application.Features.BagMovements.Rules;
using Application.Services.Repositories;
using AutoMapper;
using Domain.Entities;
using NArchitecture.Core.Application.Pipelines.Authorization;
using NArchitecture.Core.Application.Pipelines.Caching;
using NArchitecture.Core.Application.Pipelines.Logging;
using NArchitecture.Core.Application.Pipelines.Transaction;
using MediatR;
using static Application.Features.BagMovements.Constants.BagMovementsOperationClaims;

namespace Application.Features.BagMovements.Commands.Update;

public class UpdateBagMovementCommand : IRequest<UpdatedBagMovementResponse>, ISecuredRequest, ICacheRemoverRequest, ILoggableRequest, ITransactionalRequest
{
    public Guid Id { get; set; }
    public Guid BagId { get; set; }
    public Guid? FromSlotId { get; set; }
    public Guid? ToSlotId { get; set; }
    public string Action { get; set; }

    public string[] Roles => [Admin, Write, BagMovementsOperationClaims.Update];

    public bool BypassCache { get; }
    public string? CacheKey { get; }
    public string[]? CacheGroupKey => ["GetBagMovements"];

    public class UpdateBagMovementCommandHandler : IRequestHandler<UpdateBagMovementCommand, UpdatedBagMovementResponse>
    {
        private readonly IMapper _mapper;
        private readonly IBagMovementRepository _bagMovementRepository;
        private readonly BagMovementBusinessRules _bagMovementBusinessRules;

        public UpdateBagMovementCommandHandler(IMapper mapper, IBagMovementRepository bagMovementRepository,
                                         BagMovementBusinessRules bagMovementBusinessRules)
        {
            _mapper = mapper;
            _bagMovementRepository = bagMovementRepository;
            _bagMovementBusinessRules = bagMovementBusinessRules;
        }

        public async Task<UpdatedBagMovementResponse> Handle(UpdateBagMovementCommand request, CancellationToken cancellationToken)
        {
            BagMovement? bagMovement = await _bagMovementRepository.GetAsync(predicate: x => x.Id == request.Id, cancellationToken: cancellationToken);
            await _bagMovementBusinessRules.BagMovementShouldExistWhenSelected(bagMovement);
            bagMovement = _mapper.Map(request, bagMovement);

            await _bagMovementRepository.UpdateAsync(bagMovement!);

            UpdatedBagMovementResponse response = _mapper.Map<UpdatedBagMovementResponse>(bagMovement);
            return response;
        }
    }
}