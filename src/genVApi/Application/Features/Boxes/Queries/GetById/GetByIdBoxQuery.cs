using Application.Features.Boxes.Constants;
using Application.Features.Boxes.Rules;
using Application.Services.Repositories;
using AutoMapper;
using Domain.Entities;
using NArchitecture.Core.Application.Pipelines.Authorization;
using MediatR;
using static Application.Features.Boxes.Constants.BoxesOperationClaims;

namespace Application.Features.Boxes.Queries.GetById;

public class GetByIdBoxQuery : IRequest<GetByIdBoxResponse>, ISecuredRequest
{
    public Guid Id { get; set; }

    public string[] Roles => [Admin, Read];

    public class GetByIdBoxQueryHandler : IRequestHandler<GetByIdBoxQuery, GetByIdBoxResponse>
    {
        private readonly IMapper _mapper;
        private readonly IBoxRepository _boxRepository;
        private readonly BoxBusinessRules _boxBusinessRules;

        public GetByIdBoxQueryHandler(IMapper mapper, IBoxRepository boxRepository, BoxBusinessRules boxBusinessRules)
        {
            _mapper = mapper;
            _boxRepository = boxRepository;
            _boxBusinessRules = boxBusinessRules;
        }

        public async Task<GetByIdBoxResponse> Handle(GetByIdBoxQuery request, CancellationToken cancellationToken)
        {
            Box? box = await _boxRepository.GetAsync(predicate: b => b.Id == request.Id, cancellationToken: cancellationToken);
            await _boxBusinessRules.BoxShouldExistWhenSelected(box);

            GetByIdBoxResponse response = _mapper.Map<GetByIdBoxResponse>(box);
            return response;
        }
    }
}