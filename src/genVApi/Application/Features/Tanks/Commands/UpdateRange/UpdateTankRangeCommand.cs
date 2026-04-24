using Application.Features.Tanks.Constants;
using Application.Features.Tanks.Rules;
using Application.Services.Repositories;
using AutoMapper;
using Domain.Entities;
using MediatR;
using NArchitecture.Core.Application.Pipelines.Authorization;
using NArchitecture.Core.Application.Pipelines.Caching;
using NArchitecture.Core.Application.Pipelines.Logging;
using NArchitecture.Core.Application.Pipelines.Transaction;
using static Application.Features.Tanks.Constants.TanksOperationClaims;

namespace Application.Features.Tanks.Commands.UpdateRange;

public class UpdateTankRangeCommand : IRequest<UpdatedTankRangeResponse>, ISecuredRequest, ICacheRemoverRequest, ILoggableRequest, ITransactionalRequest
{
    public ICollection<UpdateTankRangeItem> Items { get; set; } = new List<UpdateTankRangeItem>();

    public string[] Roles => [Admin, Write, TanksOperationClaims.UpdateRange];

    public bool BypassCache { get; }
    public string? CacheKey { get; }
    public string[]? CacheGroupKey => ["GetTanks"];

    public class UpdateTankRangeItem
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
    }

    public class UpdateTankRangeCommandHandler : IRequestHandler<UpdateTankRangeCommand, UpdatedTankRangeResponse>
    {
        private readonly IMapper _mapper;
        private readonly ITankRepository _tankRepository;
        private readonly TankBusinessRules _tankBusinessRules;

        public UpdateTankRangeCommandHandler(IMapper mapper, ITankRepository tankRepository, TankBusinessRules tankBusinessRules)
        {
            _mapper = mapper;
            _tankRepository = tankRepository;
            _tankBusinessRules = tankBusinessRules;
        }

        public async Task<UpdatedTankRangeResponse> Handle(UpdateTankRangeCommand request, CancellationToken cancellationToken)
        {
            List<Tank> items = new List<Tank>();

            foreach (UpdateTankRangeItem item in request.Items)
            {
                Tank? entity = await _tankRepository.GetAsync(
                    predicate: x => x.Id == item.Id,
                    cancellationToken: cancellationToken
                );
                await _tankBusinessRules.TankShouldExistWhenSelected(entity);
                _mapper.Map(source: item, destination: entity!);
                items.Add(entity!);
            }

            ICollection<Tank> updated = await _tankRepository.UpdateRangeAsync(items);

            return new UpdatedTankRangeResponse { Ids = updated.Select(e => e.Id).ToList() };
        }
    }
}