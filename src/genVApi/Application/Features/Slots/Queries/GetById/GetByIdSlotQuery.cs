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
        private readonly IBagCellRepository _bagCellRepository;
        private readonly BagCellBusinessRules _bagCellBusinessRules;

        public GetByIdSlotQueryHandler(IMapper mapper, IBagCellRepository bagCellRepository, BagCellBusinessRules bagCellBusinessRules)
        {
            _mapper = mapper;
            _bagCellRepository = bagCellRepository;
            _bagCellBusinessRules = bagCellBusinessRules;
        }

        public async Task<GetByIdSlotResponse> Handle(GetByIdSlotQuery request, CancellationToken cancellationToken)
        {
            BagCell? slot = await _bagCellRepository.GetAsync(predicate: s => s.Id == request.Id, cancellationToken: cancellationToken);
            await _bagCellBusinessRules.BagCellShouldExistWhenSelected(slot);

            GetByIdSlotResponse response = _mapper.Map<GetByIdSlotResponse>(slot);
            return response;
        }
    }
}