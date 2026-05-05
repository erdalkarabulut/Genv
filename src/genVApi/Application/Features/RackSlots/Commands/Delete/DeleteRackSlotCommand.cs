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

namespace Application.Features.RackSlots.Commands.Delete;

public class DeleteRackSlotCommand : IRequest<DeletedRackSlotResponse>, ISecuredRequest, ICacheRemoverRequest, ILoggableRequest,
    ITransactionalRequest
{
    public Guid Id { get; set; }

    public string[] Roles =>
    [
        Application.Features.Racks.Constants.RacksOperationClaims.Admin,
        Application.Features.Racks.Constants.RacksOperationClaims.Write,
        Application.Features.Racks.Constants.RacksOperationClaims.Delete
    ];

    public bool BypassCache { get; }
    public string? CacheKey { get; }
    public string[]? CacheGroupKey => ["GetRackSlots", "GetCryoGrid", "GetBoxes"];

    public class Handler : IRequestHandler<DeleteRackSlotCommand, DeletedRackSlotResponse>
    {
        private readonly IMapper _mapper;
        private readonly ISlotRepository _rackSlotRepository;
        private readonly IBoxRepository _boxRepository;
        private readonly RackSlotBusinessRules _rackSlotBusinessRules;

        public Handler(
            IMapper mapper,
            ISlotRepository rackSlotRepository,
            IBoxRepository boxRepository,
            RackSlotBusinessRules rackSlotBusinessRules)
        {
            _mapper = mapper;
            _rackSlotRepository = rackSlotRepository;
            _boxRepository = boxRepository;
            _rackSlotBusinessRules = rackSlotBusinessRules;
        }

        public async Task<DeletedRackSlotResponse> Handle(DeleteRackSlotCommand request, CancellationToken cancellationToken)
        {
            Slot? slot = await _rackSlotRepository.GetAsync(
                predicate: s => s.Id == request.Id,
                cancellationToken: cancellationToken);
            await _rackSlotBusinessRules.RackSlotShouldExistWhenSelected(slot);

            bool hasBoxes = await _boxRepository.AnyAsync(
                predicate: b => b.SlotId == request.Id,
                enableTracking: false,
                cancellationToken: cancellationToken);
            if (hasBoxes)
                throw new BusinessException(
                    "Bu raf slotunda kutu bulunuyor. Önce kutuları silin veya başka bir slota taşıyın.");

            await _rackSlotRepository.DeleteAsync(slot!, permanent: true);

            return _mapper.Map<DeletedRackSlotResponse>(slot!);
        }
    }
}

public class DeletedRackSlotResponse
{
    public Guid Id { get; set; }
}
