using Application.Services.Repositories;
using AutoMapper;
using Domain.Entities;
using MediatR;
using NArchitecture.Core.Application.Pipelines.Authorization;
using NArchitecture.Core.Application.Pipelines.Caching;
using NArchitecture.Core.Application.Pipelines.Logging;
using NArchitecture.Core.Application.Pipelines.Transaction;
namespace Application.Features.RackSlots.Commands.Create;

public class CreateRackSlotCommand : IRequest<CreatedRackSlotResponse>, ISecuredRequest, ICacheRemoverRequest, ILoggableRequest,
    ITransactionalRequest
{
    public Guid RackId { get; set; }
    public string Name { get; set; } = default!;

    public string[] Roles =>
    [
        Application.Features.Racks.Constants.RacksOperationClaims.Admin,
        Application.Features.Racks.Constants.RacksOperationClaims.Write,
        Application.Features.Racks.Constants.RacksOperationClaims.Create
    ];

    public bool BypassCache { get; }
    public string? CacheKey { get; }
    public string[]? CacheGroupKey => ["GetRackSlots", "GetCryoGrid"];

    public class Handler : IRequestHandler<CreateRackSlotCommand, CreatedRackSlotResponse>
    {
        private readonly IMapper _mapper;
        private readonly ISlotRepository _rackSlotRepository;

        public Handler(IMapper mapper, ISlotRepository rackSlotRepository)
        {
            _mapper = mapper;
            _rackSlotRepository = rackSlotRepository;
        }

        public async Task<CreatedRackSlotResponse> Handle(CreateRackSlotCommand request, CancellationToken cancellationToken)
        {
            Slot entity = _mapper.Map<Slot>(request);
            await _rackSlotRepository.AddAsync(entity);
            return _mapper.Map<CreatedRackSlotResponse>(entity);
        }
    }
}

public class CreatedRackSlotResponse
{
    public Guid Id { get; set; }
    public Guid RackId { get; set; }
    public string Name { get; set; } = default!;
}
