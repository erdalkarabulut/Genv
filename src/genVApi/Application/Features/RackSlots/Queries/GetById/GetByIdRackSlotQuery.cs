using Application.Features.RackSlots.Rules;
using Application.Services.Repositories;
using AutoMapper;
using Domain.Entities;
using MediatR;
using NArchitecture.Core.Application.Pipelines.Authorization;

namespace Application.Features.RackSlots.Queries.GetById;

public class GetByIdRackSlotQuery : IRequest<GetByIdRackSlotResponse>, ISecuredRequest
{
    public Guid Id { get; set; }

    public string[] Roles =>
    [
        Application.Features.Racks.Constants.RacksOperationClaims.Admin,
        Application.Features.Racks.Constants.RacksOperationClaims.Read
    ];

    public class Handler : IRequestHandler<GetByIdRackSlotQuery, GetByIdRackSlotResponse>
    {
        private readonly IMapper _mapper;
        private readonly ISlotRepository _rackSlotRepository;
        private readonly RackSlotBusinessRules _rackSlotBusinessRules;

        public Handler(IMapper mapper, ISlotRepository rackSlotRepository, RackSlotBusinessRules rackSlotBusinessRules)
        {
            _mapper = mapper;
            _rackSlotRepository = rackSlotRepository;
            _rackSlotBusinessRules = rackSlotBusinessRules;
        }

        public async Task<GetByIdRackSlotResponse> Handle(GetByIdRackSlotQuery request, CancellationToken cancellationToken)
        {
            Slot? slot = await _rackSlotRepository.GetAsync(
                predicate: s => s.Id == request.Id,
                cancellationToken: cancellationToken);
            await _rackSlotBusinessRules.RackSlotShouldExistWhenSelected(slot);

            return _mapper.Map<GetByIdRackSlotResponse>(slot!);
        }
    }
}

public class GetByIdRackSlotResponse
{
    public Guid Id { get; set; }
    public Guid RackId { get; set; }
    public string Name { get; set; } = default!;
    public DateTime CreatedDate { get; set; }
}
