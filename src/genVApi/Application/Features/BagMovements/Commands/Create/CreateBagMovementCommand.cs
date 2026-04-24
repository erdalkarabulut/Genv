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

namespace Application.Features.BagMovements.Commands.Create;

public class CreateBagMovementCommand : IRequest<CreatedBagMovementResponse>, ISecuredRequest, ICacheRemoverRequest, ILoggableRequest, ITransactionalRequest
{
    public Guid BagId { get; set; }
    public Guid? FromSlotId { get; set; }
    public Guid? ToSlotId { get; set; }
    public string Action { get; set; }

    public string[] Roles => [Admin, Write, BagMovementsOperationClaims.Create];

    public bool BypassCache { get; }
    public string? CacheKey { get; }
    public string[]? CacheGroupKey => ["GetBagMovements"];

    public class CreateBagMovementCommandHandler : IRequestHandler<CreateBagMovementCommand, CreatedBagMovementResponse>
    {
        private readonly IMapper _mapper;
        private readonly IBagMovementRepository _bagMovementRepository;
        private readonly BagMovementBusinessRules _bagMovementBusinessRules;

        public CreateBagMovementCommandHandler(IMapper mapper, IBagMovementRepository bagMovementRepository,
                                         BagMovementBusinessRules bagMovementBusinessRules)
        {
            _mapper = mapper;
            _bagMovementRepository = bagMovementRepository;
            _bagMovementBusinessRules = bagMovementBusinessRules;
        }

        public async Task<CreatedBagMovementResponse> Handle(CreateBagMovementCommand request, CancellationToken cancellationToken)
        {
            BagMovement bagMovement = _mapper.Map<BagMovement>(request);

            await _bagMovementRepository.AddAsync(bagMovement);

            CreatedBagMovementResponse response = _mapper.Map<CreatedBagMovementResponse>(bagMovement);
            return response;
        }
    }
}