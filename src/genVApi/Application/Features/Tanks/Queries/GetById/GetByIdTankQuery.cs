using Application.Features.Tanks.Constants;
using Application.Features.Tanks.Rules;
using Application.Services.Repositories;
using AutoMapper;
using Domain.Entities;
using NArchitecture.Core.Application.Pipelines.Authorization;
using MediatR;
using static Application.Features.Tanks.Constants.TanksOperationClaims;

namespace Application.Features.Tanks.Queries.GetById;

public class GetByIdTankQuery : IRequest<GetByIdTankResponse>, ISecuredRequest
{
    public Guid Id { get; set; }

    public string[] Roles => [Admin, Read];

    public class GetByIdTankQueryHandler : IRequestHandler<GetByIdTankQuery, GetByIdTankResponse>
    {
        private readonly IMapper _mapper;
        private readonly ITankRepository _tankRepository;
        private readonly TankBusinessRules _tankBusinessRules;

        public GetByIdTankQueryHandler(IMapper mapper, ITankRepository tankRepository, TankBusinessRules tankBusinessRules)
        {
            _mapper = mapper;
            _tankRepository = tankRepository;
            _tankBusinessRules = tankBusinessRules;
        }

        public async Task<GetByIdTankResponse> Handle(GetByIdTankQuery request, CancellationToken cancellationToken)
        {
            Tank? tank = await _tankRepository.GetAsync(predicate: t => t.Id == request.Id, cancellationToken: cancellationToken);
            await _tankBusinessRules.TankShouldExistWhenSelected(tank);

            GetByIdTankResponse response = _mapper.Map<GetByIdTankResponse>(tank);
            return response;
        }
    }
}