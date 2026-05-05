using Application.Features.Donors.Constants;
using Application.Features.Donors.Rules;
using Application.Services.Repositories;
using AutoMapper;
using Domain.Entities;
using NArchitecture.Core.Application.Pipelines.Authorization;
using NArchitecture.Core.Application.Pipelines.Caching;
using NArchitecture.Core.Application.Pipelines.Logging;
using NArchitecture.Core.Application.Pipelines.Transaction;
using MediatR;
using static Application.Features.Donors.Constants.DonorsOperationClaims;

namespace Application.Features.Donors.Commands.Create;

public class CreateDonorCommand : IRequest<CreatedDonorResponse>, ISecuredRequest, ICacheRemoverRequest, ILoggableRequest, ITransactionalRequest
{
    public string FullName { get; set; } = default!;
    public double WeightKg { get; set; }
    public string? BloodGroup { get; set; }
    public string? Relation { get; set; }
    public string? IdentityNumber { get; set; }
    public Domain.Enums.DonorType DonorType { get; set; } = Domain.Enums.DonorType.Related;
    public DateTime? BirthDate { get; set; }

    public string[] Roles => [Admin, Write, DonorsOperationClaims.Create];

    public bool BypassCache { get; }
    public string? CacheKey { get; }
    public string[]? CacheGroupKey => ["GetDonors"];

    public class CreateDonorCommandHandler : IRequestHandler<CreateDonorCommand, CreatedDonorResponse>
    {
        private readonly IMapper _mapper;
        private readonly IDonorRepository _donorRepository;
        private readonly DonorBusinessRules _donorBusinessRules;

        public CreateDonorCommandHandler(IMapper mapper, IDonorRepository donorRepository,
                                         DonorBusinessRules donorBusinessRules)
        {
            _mapper = mapper;
            _donorRepository = donorRepository;
            _donorBusinessRules = donorBusinessRules;
        }

        public async Task<CreatedDonorResponse> Handle(CreateDonorCommand request, CancellationToken cancellationToken)
        {
            Donor donor = _mapper.Map<Donor>(request);

            await _donorRepository.AddAsync(donor);

            CreatedDonorResponse response = _mapper.Map<CreatedDonorResponse>(donor);
            return response;
        }
    }
}