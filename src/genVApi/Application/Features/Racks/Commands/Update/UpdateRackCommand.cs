using Application.Features.Racks.Constants;
using Application.Features.Racks.Rules;
using Application.Services.Repositories;
using AutoMapper;
using Domain.Entities;
using NArchitecture.Core.Application.Pipelines.Authorization;
using NArchitecture.Core.Application.Pipelines.Caching;
using NArchitecture.Core.Application.Pipelines.Logging;
using NArchitecture.Core.Application.Pipelines.Transaction;
using MediatR;
using static Application.Features.Racks.Constants.RacksOperationClaims;

namespace Application.Features.Racks.Commands.Update;

public class UpdateRackCommand : IRequest<UpdatedRackResponse>, ISecuredRequest, ICacheRemoverRequest, ILoggableRequest, ITransactionalRequest
{
    public Guid Id { get; set; }
    public Guid TankId { get; set; }
    public string Name { get; set; }

    public string[] Roles => [Admin, Write, RacksOperationClaims.Update];

    public bool BypassCache { get; }
    public string? CacheKey { get; }
    public string[]? CacheGroupKey => ["GetRacks"];

    public class UpdateRackCommandHandler : IRequestHandler<UpdateRackCommand, UpdatedRackResponse>
    {
        private readonly IMapper _mapper;
        private readonly IRackRepository _rackRepository;
        private readonly RackBusinessRules _rackBusinessRules;

        public UpdateRackCommandHandler(IMapper mapper, IRackRepository rackRepository,
                                         RackBusinessRules rackBusinessRules)
        {
            _mapper = mapper;
            _rackRepository = rackRepository;
            _rackBusinessRules = rackBusinessRules;
        }

        public async Task<UpdatedRackResponse> Handle(UpdateRackCommand request, CancellationToken cancellationToken)
        {
            Rack? rack = await _rackRepository.GetAsync(predicate: x => x.Id == request.Id, cancellationToken: cancellationToken);
            await _rackBusinessRules.RackShouldExistWhenSelected(rack);
            rack = _mapper.Map(request, rack);

            await _rackRepository.UpdateAsync(rack!);

            UpdatedRackResponse response = _mapper.Map<UpdatedRackResponse>(rack);
            return response;
        }
    }
}