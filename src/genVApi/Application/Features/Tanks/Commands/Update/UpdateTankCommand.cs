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

namespace Application.Features.Tanks.Commands.Update;

public class UpdateTankCommand : IRequest<UpdatedTankResponse>, ISecuredRequest, ICacheRemoverRequest, ILoggableRequest, ITransactionalRequest
{
    public Guid Id { get; set; }
    public string Name { get; set; }

    public string[] Roles => [Admin, Write, TanksOperationClaims.Update];

    public bool BypassCache { get; }
    public string? CacheKey { get; }
    public string[]? CacheGroupKey => ["GetTanks"];

    public class UpdateTankCommandHandler : IRequestHandler<UpdateTankCommand, UpdatedTankResponse>
    {
        private readonly IMapper _mapper;
        private readonly ITankRepository _tankRepository;
        private readonly TankBusinessRules _tankBusinessRules;

        public UpdateTankCommandHandler(IMapper mapper, ITankRepository tankRepository,
                                         TankBusinessRules tankBusinessRules)
        {
            _mapper = mapper;
            _tankRepository = tankRepository;
            _tankBusinessRules = tankBusinessRules;
        }

        public async Task<UpdatedTankResponse> Handle(UpdateTankCommand request, CancellationToken cancellationToken)
        {
            Tank? tank = await _tankRepository.GetAsync(predicate: x => x.Id == request.Id, cancellationToken: cancellationToken);
            await _tankBusinessRules.TankShouldExistWhenSelected(tank);
            tank = _mapper.Map(request, tank);

            await _tankRepository.UpdateAsync(tank!);

            UpdatedTankResponse response = _mapper.Map<UpdatedTankResponse>(tank);
            return response;
        }
    }
}