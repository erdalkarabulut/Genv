using Application.Features.Slots.Constants;
using Application.Features.Slots.Rules;
using Application.Services.Repositories;
using AutoMapper;
using Domain.Entities;
using NArchitecture.Core.Application.Pipelines.Authorization;
using MediatR;
using static Application.Features.Slots.Constants.SlotsOperationClaims;

namespace Application.Features.Slots.Queries.GetById;

public class GetByIdSlotQuery : IRequest<GetByIdSlotResponse>, ISecuredRequest
{
    public Guid Id { get; set; }

    public string[] Roles => [Admin, Read];

    public class GetByIdSlotQueryHandler : IRequestHandler<GetByIdSlotQuery, GetByIdSlotResponse>
    {
        private readonly IMapper _mapper;
        private readonly ISlotRepository _slotRepository;
        private readonly SlotBusinessRules _slotBusinessRules;

        public GetByIdSlotQueryHandler(IMapper mapper, ISlotRepository slotRepository, SlotBusinessRules slotBusinessRules)
        {
            _mapper = mapper;
            _slotRepository = slotRepository;
            _slotBusinessRules = slotBusinessRules;
        }

        public async Task<GetByIdSlotResponse> Handle(GetByIdSlotQuery request, CancellationToken cancellationToken)
        {
            Slot? slot = await _slotRepository.GetAsync(predicate: s => s.Id == request.Id, cancellationToken: cancellationToken);
            await _slotBusinessRules.SlotShouldExistWhenSelected(slot);

            GetByIdSlotResponse response = _mapper.Map<GetByIdSlotResponse>(slot);
            return response;
        }
    }
}