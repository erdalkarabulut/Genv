using Application.Features.BagMovements.Constants;
using Application.Features.BagMovements.Rules;
using Application.Services.Repositories;
using AutoMapper;
using Domain.Entities;
using NArchitecture.Core.Application.Pipelines.Authorization;
using MediatR;
using static Application.Features.BagMovements.Constants.BagMovementsOperationClaims;

namespace Application.Features.BagMovements.Queries.GetById;

public class GetByIdBagMovementQuery : IRequest<GetByIdBagMovementResponse>, ISecuredRequest
{
    public Guid Id { get; set; }

    public string[] Roles => [Admin, Read];

    public class GetByIdBagMovementQueryHandler : IRequestHandler<GetByIdBagMovementQuery, GetByIdBagMovementResponse>
    {
        private readonly IMapper _mapper;
        private readonly IBagMovementRepository _bagMovementRepository;
        private readonly BagMovementBusinessRules _bagMovementBusinessRules;

        public GetByIdBagMovementQueryHandler(IMapper mapper, IBagMovementRepository bagMovementRepository, BagMovementBusinessRules bagMovementBusinessRules)
        {
            _mapper = mapper;
            _bagMovementRepository = bagMovementRepository;
            _bagMovementBusinessRules = bagMovementBusinessRules;
        }

        public async Task<GetByIdBagMovementResponse> Handle(GetByIdBagMovementQuery request, CancellationToken cancellationToken)
        {
            BagMovement? bagMovement = await _bagMovementRepository.GetAsync(predicate: bm => bm.Id == request.Id, cancellationToken: cancellationToken);
            await _bagMovementBusinessRules.BagMovementShouldExistWhenSelected(bagMovement);

            GetByIdBagMovementResponse response = _mapper.Map<GetByIdBagMovementResponse>(bagMovement);
            return response;
        }
    }
}