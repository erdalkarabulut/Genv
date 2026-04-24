using Application.Features.Donors.Constants;
using Application.Features.Donors.Rules;
using Application.Services.Repositories;
using AutoMapper;
using Domain.Entities;
using NArchitecture.Core.Application.Pipelines.Authorization;
using MediatR;
using static Application.Features.Donors.Constants.DonorsOperationClaims;

namespace Application.Features.Donors.Queries.GetById;

public class GetByIdDonorQuery : IRequest<GetByIdDonorResponse>, ISecuredRequest
{
    public Guid Id { get; set; }

    public string[] Roles => [Admin, Read];

    public class GetByIdDonorQueryHandler : IRequestHandler<GetByIdDonorQuery, GetByIdDonorResponse>
    {
        private readonly IMapper _mapper;
        private readonly IDonorRepository _donorRepository;
        private readonly DonorBusinessRules _donorBusinessRules;

        public GetByIdDonorQueryHandler(IMapper mapper, IDonorRepository donorRepository, DonorBusinessRules donorBusinessRules)
        {
            _mapper = mapper;
            _donorRepository = donorRepository;
            _donorBusinessRules = donorBusinessRules;
        }

        public async Task<GetByIdDonorResponse> Handle(GetByIdDonorQuery request, CancellationToken cancellationToken)
        {
            Donor? donor = await _donorRepository.GetAsync(predicate: d => d.Id == request.Id, cancellationToken: cancellationToken);
            await _donorBusinessRules.DonorShouldExistWhenSelected(donor);

            GetByIdDonorResponse response = _mapper.Map<GetByIdDonorResponse>(donor);
            return response;
        }
    }
}