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

namespace Application.Features.Racks.Commands.Create;

public class CreateRackCommand : IRequest<CreatedRackResponse>, ISecuredRequest, ICacheRemoverRequest, ILoggableRequest, ITransactionalRequest
{
    public Guid TankId { get; set; }
    public string Name { get; set; }

    public string[] Roles => [Admin, Write, RacksOperationClaims.Create];

    public bool BypassCache { get; }
    public string? CacheKey { get; }
    public string[]? CacheGroupKey => ["GetRacks"];

    public class CreateRackCommandHandler : IRequestHandler<CreateRackCommand, CreatedRackResponse>
    {
        private readonly IMapper _mapper;
        private readonly IRackRepository _rackRepository;
        private readonly RackBusinessRules _rackBusinessRules;

        public CreateRackCommandHandler(IMapper mapper, IRackRepository rackRepository,
                                         RackBusinessRules rackBusinessRules)
        {
            _mapper = mapper;
            _rackRepository = rackRepository;
            _rackBusinessRules = rackBusinessRules;
        }

        public async Task<CreatedRackResponse> Handle(CreateRackCommand request, CancellationToken cancellationToken)
        {
            Rack rack = _mapper.Map<Rack>(request);

            await _rackRepository.AddAsync(rack);

            CreatedRackResponse response = _mapper.Map<CreatedRackResponse>(rack);
            return response;
        }
    }
}