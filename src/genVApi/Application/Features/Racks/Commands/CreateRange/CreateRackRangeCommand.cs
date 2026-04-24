using Application.Features.Racks.Constants;
using Application.Services.Repositories;
using AutoMapper;
using Domain.Entities;
using MediatR;
using NArchitecture.Core.Application.Pipelines.Authorization;
using NArchitecture.Core.Application.Pipelines.Caching;
using NArchitecture.Core.Application.Pipelines.Logging;
using NArchitecture.Core.Application.Pipelines.Transaction;
using static Application.Features.Racks.Constants.RacksOperationClaims;

namespace Application.Features.Racks.Commands.CreateRange;

public class CreateRackRangeCommand : IRequest<CreatedRackRangeResponse>, ISecuredRequest, ICacheRemoverRequest, ILoggableRequest, ITransactionalRequest
{
    public ICollection<CreateRackRangeItem> Items { get; set; } = new List<CreateRackRangeItem>();

    public string[] Roles => [Admin, Write, RacksOperationClaims.CreateRange];

    public bool BypassCache { get; }
    public string? CacheKey { get; }
    public string[]? CacheGroupKey => ["GetRacks"];

    public class CreateRackRangeItem
    {
        public Guid TankId { get; set; }
        public string Name { get; set; }
    }

    public class CreateRackRangeCommandHandler : IRequestHandler<CreateRackRangeCommand, CreatedRackRangeResponse>
    {
        private readonly IMapper _mapper;
        private readonly IRackRepository _rackRepository;

        public CreateRackRangeCommandHandler(IMapper mapper, IRackRepository rackRepository)
        {
            _mapper = mapper;
            _rackRepository = rackRepository;
        }

        public async Task<CreatedRackRangeResponse> Handle(CreateRackRangeCommand request, CancellationToken cancellationToken)
        {
            List<Rack> racks = request.Items.Select(item => _mapper.Map<Rack>(item)).ToList();

            ICollection<Rack> addedRacks = await _rackRepository.AddRangeAsync(racks);

            return new CreatedRackRangeResponse { Ids = addedRacks.Select(e => e.Id).ToList() };
        }
    }
}