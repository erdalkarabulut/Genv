using Application.Features.BagMovements.Constants;
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

namespace Application.Features.BagMovements.Commands.Delete;

public class DeleteBagMovementCommand : IRequest<DeletedBagMovementResponse>, ISecuredRequest, ICacheRemoverRequest, ILoggableRequest, ITransactionalRequest
{
    public Guid Id { get; set; }

    public string[] Roles => [Admin, Write, BagMovementsOperationClaims.Delete];

    public bool BypassCache { get; }
    public string? CacheKey { get; }
    public string[]? CacheGroupKey => ["GetBagMovements"];

    public class DeleteBagMovementCommandHandler : IRequestHandler<DeleteBagMovementCommand, DeletedBagMovementResponse>
    {
        private readonly IMapper _mapper;
        private readonly IBagMovementRepository _bagMovementRepository;
        private readonly BagMovementBusinessRules _bagMovementBusinessRules;

        public DeleteBagMovementCommandHandler(IMapper mapper, IBagMovementRepository bagMovementRepository,
                                         BagMovementBusinessRules bagMovementBusinessRules)
        {
            _mapper = mapper;
            _bagMovementRepository = bagMovementRepository;
            _bagMovementBusinessRules = bagMovementBusinessRules;
        }

        public async Task<DeletedBagMovementResponse> Handle(DeleteBagMovementCommand request, CancellationToken cancellationToken)
        {
            BagMovement? bagMovement = await _bagMovementRepository.GetAsync(predicate: bm => bm.Id == request.Id, cancellationToken: cancellationToken);
            await _bagMovementBusinessRules.BagMovementShouldExistWhenSelected(bagMovement);

            await _bagMovementRepository.DeleteAsync(bagMovement!);

            DeletedBagMovementResponse response = _mapper.Map<DeletedBagMovementResponse>(bagMovement);
            return response;
        }
    }
}