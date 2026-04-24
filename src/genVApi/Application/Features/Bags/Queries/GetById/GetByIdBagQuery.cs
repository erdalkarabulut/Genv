using Application.Features.Bags.Constants;
using Application.Features.Bags.Rules;
using Application.Services.Repositories;
using AutoMapper;
using Domain.Entities;
using NArchitecture.Core.Application.Pipelines.Authorization;
using MediatR;
using static Application.Features.Bags.Constants.BagsOperationClaims;

namespace Application.Features.Bags.Queries.GetById;

public class GetByIdBagQuery : IRequest<GetByIdBagResponse>, ISecuredRequest
{
    public Guid Id { get; set; }

    public string[] Roles => [Admin, Read];

    public class GetByIdBagQueryHandler : IRequestHandler<GetByIdBagQuery, GetByIdBagResponse>
    {
        private readonly IMapper _mapper;
        private readonly IBagRepository _bagRepository;
        private readonly BagBusinessRules _bagBusinessRules;

        public GetByIdBagQueryHandler(IMapper mapper, IBagRepository bagRepository, BagBusinessRules bagBusinessRules)
        {
            _mapper = mapper;
            _bagRepository = bagRepository;
            _bagBusinessRules = bagBusinessRules;
        }

        public async Task<GetByIdBagResponse> Handle(GetByIdBagQuery request, CancellationToken cancellationToken)
        {
            Bag? bag = await _bagRepository.GetAsync(predicate: b => b.Id == request.Id, cancellationToken: cancellationToken);
            await _bagBusinessRules.BagShouldExistWhenSelected(bag);

            GetByIdBagResponse response = _mapper.Map<GetByIdBagResponse>(bag);
            return response;
        }
    }
}