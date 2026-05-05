using Application.Features.Donors.Constants;
using Application.Services.Repositories;
using AutoMapper;
using Domain.Entities;
using MediatR;
using NArchitecture.Core.Application.Pipelines.Authorization;
using NArchitecture.Core.Application.Pipelines.Caching;
using NArchitecture.Core.Application.Pipelines.Logging;
using NArchitecture.Core.Application.Pipelines.Transaction;
using static Application.Features.Donors.Constants.DonorsOperationClaims;

namespace Application.Features.Donors.Commands.CreateRange;

public class CreateDonorRangeCommand : IRequest<CreatedDonorRangeResponse>, ISecuredRequest, ICacheRemoverRequest, ILoggableRequest, ITransactionalRequest
{
    public ICollection<CreateDonorRangeItem> Items { get; set; } = new List<CreateDonorRangeItem>();

    public string[] Roles => [Admin, Write, DonorsOperationClaims.CreateRange];

    public bool BypassCache { get; }
    public string? CacheKey { get; }
    public string[]? CacheGroupKey => ["GetDonors"];

    public class CreateDonorRangeItem
    {
        public string FullName { get; set; }
        public double WeightKg { get; set; }
        public string? BloodGroup { get; set; }
        public string? Relation { get; set; }
        public string? IdentityNumber { get; set; }
    }

    public class CreateDonorRangeCommandHandler : IRequestHandler<CreateDonorRangeCommand, CreatedDonorRangeResponse>
    {
        private readonly IMapper _mapper;
        private readonly IDonorRepository _donorRepository;

        public CreateDonorRangeCommandHandler(IMapper mapper, IDonorRepository donorRepository)
        {
            _mapper = mapper;
            _donorRepository = donorRepository;
        }

        public async Task<CreatedDonorRangeResponse> Handle(CreateDonorRangeCommand request, CancellationToken cancellationToken)
        {
            List<Donor> donors = request.Items.Select(item => _mapper.Map<Donor>(item)).ToList();

            ICollection<Donor> addedDonors = await _donorRepository.AddRangeAsync(donors);

            return new CreatedDonorRangeResponse { Ids = addedDonors.Select(e => e.Id).ToList() };
        }
    }
}