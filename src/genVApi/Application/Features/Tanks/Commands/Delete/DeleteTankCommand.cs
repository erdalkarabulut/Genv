using Application.Features.Tanks.Constants;
using Application.Features.Tanks.Constants;
using Application.Features.Tanks.Rules;
using Application.Services.Repositories;
using AutoMapper;
using Domain.Entities;
using NArchitecture.Core.Application.Pipelines.Authorization;
using NArchitecture.Core.Application.Pipelines.Caching;
using NArchitecture.Core.Application.Pipelines.Logging;
using NArchitecture.Core.Application.Pipelines.Transaction;
using MediatR;
using static Application.Features.Tanks.Constants.TanksOperationClaims;

namespace Application.Features.Tanks.Commands.Delete;

public class DeleteTankCommand : IRequest<DeletedTankResponse>, ISecuredRequest, ICacheRemoverRequest, ILoggableRequest, ITransactionalRequest
{
    public Guid Id { get; set; }

    public string[] Roles => [Admin, Write, TanksOperationClaims.Delete];

    public bool BypassCache { get; }
    public string? CacheKey { get; }
    public string[]? CacheGroupKey => ["GetTanks"];

    public class DeleteTankCommandHandler : IRequestHandler<DeleteTankCommand, DeletedTankResponse>
    {
        private readonly IMapper _mapper;
        private readonly ITankRepository _tankRepository;
        private readonly TankBusinessRules _tankBusinessRules;

        public DeleteTankCommandHandler(IMapper mapper, ITankRepository tankRepository,
                                         TankBusinessRules tankBusinessRules)
        {
            _mapper = mapper;
            _tankRepository = tankRepository;
            _tankBusinessRules = tankBusinessRules;
        }

        public async Task<DeletedTankResponse> Handle(DeleteTankCommand request, CancellationToken cancellationToken)
        {
            Tank? tank = await _tankRepository.GetAsync(predicate: t => t.Id == request.Id, cancellationToken: cancellationToken);
            await _tankBusinessRules.TankShouldExistWhenSelected(tank);

            await _tankRepository.DeleteAsync(tank!);

            DeletedTankResponse response = _mapper.Map<DeletedTankResponse>(tank);
            return response;
        }
    }
}