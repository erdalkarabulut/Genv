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

namespace Application.Features.Donors.Commands.Update;

public class UpdateDonorCommand : IRequest<UpdatedDonorResponse>, ISecuredRequest, ICacheRemoverRequest, ILoggableRequest, ITransactionalRequest
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = default!;
    public double WeightKg { get; set; }
    public string? BloodGroup { get; set; }
    public string? Relation { get; set; }
    public Domain.Enums.DonorType DonorType { get; set; } = Domain.Enums.DonorType.Related;
    public DateTime? BirthDate { get; set; }

    public string[] Roles => [Admin, Write, DonorsOperationClaims.Update];

    public bool BypassCache { get; }
    public string? CacheKey { get; }
    public string[]? CacheGroupKey => ["GetDonors"];

    public class UpdateDonorCommandHandler : IRequestHandler<UpdateDonorCommand, UpdatedDonorResponse>
    {
        private readonly IMapper _mapper;
        private readonly IDonorRepository _donorRepository;
        private readonly DonorBusinessRules _donorBusinessRules;

        public UpdateDonorCommandHandler(IMapper mapper, IDonorRepository donorRepository,
                                         DonorBusinessRules donorBusinessRules)
        {
            _mapper = mapper;
            _donorRepository = donorRepository;
            _donorBusinessRules = donorBusinessRules;
        }

        public async Task<UpdatedDonorResponse> Handle(UpdateDonorCommand request, CancellationToken cancellationToken)
        {
            Donor? donor = await _donorRepository.GetAsync(predicate: x => x.Id == request.Id, cancellationToken: cancellationToken);
            await _donorBusinessRules.DonorShouldExistWhenSelected(donor);
            donor = _mapper.Map(request, donor);

            await _donorRepository.UpdateAsync(donor!);

            UpdatedDonorResponse response = _mapper.Map<UpdatedDonorResponse>(donor);
            return response;
        }
    }
}