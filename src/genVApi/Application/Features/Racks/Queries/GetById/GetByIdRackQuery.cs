using Application.Features.Racks.Constants;
using Application.Features.Racks.Rules;
using Application.Services.Repositories;
using AutoMapper;
using Domain.Entities;
using NArchitecture.Core.Application.Pipelines.Authorization;
using MediatR;
using static Application.Features.Racks.Constants.RacksOperationClaims;

namespace Application.Features.Racks.Queries.GetById;

public class GetByIdRackQuery : IRequest<GetByIdRackResponse>, ISecuredRequest
{
    public Guid Id { get; set; }

    public string[] Roles => [Admin, Read];

    public class GetByIdRackQueryHandler : IRequestHandler<GetByIdRackQuery, GetByIdRackResponse>
    {
        private readonly IMapper _mapper;
        private readonly IRackRepository _rackRepository;
        private readonly RackBusinessRules _rackBusinessRules;

        public GetByIdRackQueryHandler(IMapper mapper, IRackRepository rackRepository, RackBusinessRules rackBusinessRules)
        {
            _mapper = mapper;
            _rackRepository = rackRepository;
            _rackBusinessRules = rackBusinessRules;
        }

        public async Task<GetByIdRackResponse> Handle(GetByIdRackQuery request, CancellationToken cancellationToken)
        {
            Rack? rack = await _rackRepository.GetAsync(predicate: r => r.Id == request.Id, cancellationToken: cancellationToken);
            await _rackBusinessRules.RackShouldExistWhenSelected(rack);

            GetByIdRackResponse response = _mapper.Map<GetByIdRackResponse>(rack);
            return response;
        }
    }
}