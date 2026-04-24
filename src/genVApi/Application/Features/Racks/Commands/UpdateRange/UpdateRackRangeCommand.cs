using Application.Features.Racks.Constants;
using Application.Features.Racks.Rules;
using Application.Services.Repositories;
using AutoMapper;
using Domain.Entities;
using MediatR;
using NArchitecture.Core.Application.Pipelines.Authorization;
using NArchitecture.Core.Application.Pipelines.Caching;
using NArchitecture.Core.Application.Pipelines.Logging;
using NArchitecture.Core.Application.Pipelines.Transaction;
using static Application.Features.Racks.Constants.RacksOperationClaims;

namespace Application.Features.Racks.Commands.UpdateRange;

public class UpdateRackRangeCommand : IRequest<UpdatedRackRangeResponse>, ISecuredRequest, ICacheRemoverRequest, ILoggableRequest, ITransactionalRequest
{
    public ICollection<UpdateRackRangeItem> Items { get; set; } = new List<UpdateRackRangeItem>();

    public string[] Roles => [Admin, Write, RacksOperationClaims.UpdateRange];

    public bool BypassCache { get; }
    public string? CacheKey { get; }
    public string[]? CacheGroupKey => ["GetRacks"];

    public class UpdateRackRangeItem
    {
        public Guid Id { get; set; }
        public Guid TankId { get; set; }
        public string Name { get; set; }
    }

    public class UpdateRackRangeCommandHandler : IRequestHandler<UpdateRackRangeCommand, UpdatedRackRangeResponse>
    {
        private readonly IMapper _mapper;
        private readonly IRackRepository _rackRepository;
        private readonly RackBusinessRules _rackBusinessRules;

        public UpdateRackRangeCommandHandler(IMapper mapper, IRackRepository rackRepository, RackBusinessRules rackBusinessRules)
        {
            _mapper = mapper;
            _rackRepository = rackRepository;
            _rackBusinessRules = rackBusinessRules;
        }

        public async Task<UpdatedRackRangeResponse> Handle(UpdateRackRangeCommand request, CancellationToken cancellationToken)
        {
            List<Rack> items = new List<Rack>();

            foreach (UpdateRackRangeItem item in request.Items)
            {
                Rack? entity = await _rackRepository.GetAsync(
                    predicate: x => x.Id == item.Id,
                    cancellationToken: cancellationToken
                );
                await _rackBusinessRules.RackShouldExistWhenSelected(entity);
                _mapper.Map(source: item, destination: entity!);
                items.Add(entity!);
            }

            ICollection<Rack> updated = await _rackRepository.UpdateRangeAsync(items);

            return new UpdatedRackRangeResponse { Ids = updated.Select(e => e.Id).ToList() };
        }
    }
}