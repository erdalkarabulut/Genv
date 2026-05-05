using Application.Features.Racks.Constants;
using Application.Features.Racks.Constants;
using Application.Features.Racks.Rules;
using Application.Services.Repositories;
using AutoMapper;
using Domain.Entities;
using NArchitecture.Core.Application.Pipelines.Authorization;
using NArchitecture.Core.Application.Pipelines.Caching;
using NArchitecture.Core.Application.Pipelines.Logging;
using NArchitecture.Core.Application.Pipelines.Transaction;
using NArchitecture.Core.CrossCuttingConcerns.Exception.Types;
using MediatR;
using static Application.Features.Racks.Constants.RacksOperationClaims;

namespace Application.Features.Racks.Commands.Delete;

public class DeleteRackCommand : IRequest<DeletedRackResponse>, ISecuredRequest, ICacheRemoverRequest, ILoggableRequest, ITransactionalRequest
{
    public Guid Id { get; set; }

    public string[] Roles => [Admin, Write, RacksOperationClaims.Delete];

    public bool BypassCache { get; }
    public string? CacheKey { get; }
    public string[]? CacheGroupKey => ["GetRacks"];

    public class DeleteRackCommandHandler : IRequestHandler<DeleteRackCommand, DeletedRackResponse>
    {
        private readonly IMapper _mapper;
        private readonly IRackRepository _rackRepository;
        private readonly ISlotRepository _rackSlotRepository;
        private readonly RackBusinessRules _rackBusinessRules;

        public DeleteRackCommandHandler(IMapper mapper, IRackRepository rackRepository,
                                         ISlotRepository rackSlotRepository,
                                         RackBusinessRules rackBusinessRules)
        {
            _mapper = mapper;
            _rackRepository = rackRepository;
            _rackSlotRepository = rackSlotRepository;
            _rackBusinessRules = rackBusinessRules;
        }

        public async Task<DeletedRackResponse> Handle(DeleteRackCommand request, CancellationToken cancellationToken)
        {
            Rack? rack = await _rackRepository.GetAsync(predicate: r => r.Id == request.Id, cancellationToken: cancellationToken);
            await _rackBusinessRules.RackShouldExistWhenSelected(rack);

            bool hasSlots = await _rackSlotRepository.AnyAsync(
                predicate: s => s.RackId == request.Id,
                enableTracking: false,
                cancellationToken: cancellationToken);
            if (hasSlots)
                throw new BusinessException("Bu rafa bağlı slotlar var. Önce slotları silin.");

            await _rackRepository.DeleteAsync(rack!, permanent: true);

            DeletedRackResponse response = _mapper.Map<DeletedRackResponse>(rack);
            return response;
        }
    }
}