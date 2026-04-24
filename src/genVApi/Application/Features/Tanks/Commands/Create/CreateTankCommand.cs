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

namespace Application.Features.Tanks.Commands.Create;

public class CreateTankCommand : IRequest<CreatedTankResponse>, ISecuredRequest, ICacheRemoverRequest, ILoggableRequest, ITransactionalRequest
{
    public string Name { get; set; }

    public string[] Roles => [Admin, Write, TanksOperationClaims.Create];

    public bool BypassCache { get; }
    public string? CacheKey { get; }
    public string[]? CacheGroupKey => ["GetTanks"];

    public class CreateTankCommandHandler : IRequestHandler<CreateTankCommand, CreatedTankResponse>
    {
        private readonly IMapper _mapper;
        private readonly ITankRepository _tankRepository;
        private readonly TankBusinessRules _tankBusinessRules;

        public CreateTankCommandHandler(IMapper mapper, ITankRepository tankRepository,
                                         TankBusinessRules tankBusinessRules)
        {
            _mapper = mapper;
            _tankRepository = tankRepository;
            _tankBusinessRules = tankBusinessRules;
        }

        public async Task<CreatedTankResponse> Handle(CreateTankCommand request, CancellationToken cancellationToken)
        {
            Tank tank = _mapper.Map<Tank>(request);

            await _tankRepository.AddAsync(tank);

            CreatedTankResponse response = _mapper.Map<CreatedTankResponse>(tank);
            return response;
        }
    }
}