using Application.Features.RackSlots.Rules;
using Application.Services.Repositories;
using AutoMapper;
using Domain.Entities;
using MediatR;
using NArchitecture.Core.Application.Pipelines.Authorization;
using NArchitecture.Core.Application.Pipelines.Caching;
using NArchitecture.Core.Application.Pipelines.Logging;
using NArchitecture.Core.Application.Pipelines.Transaction;
using NArchitecture.Core.CrossCuttingConcerns.Exception.Types;

namespace Application.Features.RackSlots.Commands.Update;

public class UpdateRackSlotCommand : IRequest<UpdatedRackSlotResponse>, ISecuredRequest, ICacheRemoverRequest, ILoggableRequest,
    ITransactionalRequest
{
    public Guid Id { get; set; }
    public Guid RackId { get; set; }
    public string Name { get; set; } = default!;

    public string[] Roles =>
    [
        Application.Features.Racks.Constants.RacksOperationClaims.Admin,
        Application.Features.Racks.Constants.RacksOperationClaims.Write,
        Application.Features.Racks.Constants.RacksOperationClaims.Update
    ];

    public bool BypassCache { get; }
    public string? CacheKey { get; }
    public string[]? CacheGroupKey => ["GetRackSlots", "GetCryoGrid"];

    public class Handler : IRequestHandler<UpdateRackSlotCommand, UpdatedRackSlotResponse>
    {
        private readonly IMapper _mapper;
        private readonly ISlotRepository _rackSlotRepository;
        private readonly IRackRepository _rackRepository;
        private readonly RackSlotBusinessRules _rackSlotBusinessRules;

        public Handler(
            IMapper mapper,
            ISlotRepository rackSlotRepository,
            IRackRepository rackRepository,
            RackSlotBusinessRules rackSlotBusinessRules)
        {
            _mapper = mapper;
            _rackSlotRepository = rackSlotRepository;
            _rackRepository = rackRepository;
            _rackSlotBusinessRules = rackSlotBusinessRules;
        }

        public async Task<UpdatedRackSlotResponse> Handle(UpdateRackSlotCommand request, CancellationToken cancellationToken)
        {
            Slot? slot = await _rackSlotRepository.GetAsync(
                predicate: s => s.Id == request.Id,
                cancellationToken: cancellationToken);
            await _rackSlotBusinessRules.RackSlotShouldExistWhenSelected(slot);

            Rack? rack = await _rackRepository.GetAsync(
                predicate: r => r.Id == request.RackId,
                enableTracking: false,
                cancellationToken: cancellationToken);
            if (rack is null)
                throw new BusinessException("Seçilen rack bulunamadı.");

            bool duplicate = await _rackSlotRepository.AnyAsync(
                predicate: s => s.RackId == request.RackId && s.Name == request.Name && s.Id != request.Id,
                enableTracking: false,
                cancellationToken: cancellationToken);
            if (duplicate)
                throw new BusinessException("Bu rack üzerinde aynı ada sahip başka bir raf slotu zaten var.");

            Slot updated = _mapper.Map(request, slot);
            await _rackSlotRepository.UpdateAsync(updated);

            return _mapper.Map<UpdatedRackSlotResponse>(updated);
        }
    }
}

public class UpdatedRackSlotResponse
{
    public Guid Id { get; set; }
    public Guid RackId { get; set; }
    public string Name { get; set; } = default!;
}
