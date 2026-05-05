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
using NArchitecture.Core.CrossCuttingConcerns.Exception.Types;
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
        private readonly IRackRepository _rackRepository;
        private readonly TankBusinessRules _tankBusinessRules;

        public DeleteTankCommandHandler(IMapper mapper, ITankRepository tankRepository,
                                         IRackRepository rackRepository,
                                         TankBusinessRules tankBusinessRules)
        {
            _mapper = mapper;
            _tankRepository = tankRepository;
            _rackRepository = rackRepository;
            _tankBusinessRules = tankBusinessRules;
        }

        public async Task<DeletedTankResponse> Handle(DeleteTankCommand request, CancellationToken cancellationToken)
        {
            Tank? tank = await _tankRepository.GetAsync(predicate: t => t.Id == request.Id, cancellationToken: cancellationToken);
            await _tankBusinessRules.TankShouldExistWhenSelected(tank);

            bool hasRacks = await _rackRepository.AnyAsync(
                predicate: r => r.TankId == request.Id,
                enableTracking: false,
                cancellationToken: cancellationToken);
            if (hasRacks)
                throw new BusinessException("Bu tanka bağlı raflar var. Önce rafları silin.");

            await _tankRepository.DeleteAsync(tank!, permanent: true);

            DeletedTankResponse response = _mapper.Map<DeletedTankResponse>(tank);
            return response;
        }
    }
}