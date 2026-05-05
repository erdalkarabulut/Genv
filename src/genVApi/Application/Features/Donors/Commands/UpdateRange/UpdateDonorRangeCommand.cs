using Application.Features.Donors.Constants;
using Application.Features.Donors.Rules;
using Application.Services.Repositories;
using AutoMapper;
using Domain.Entities;
using MediatR;
using NArchitecture.Core.Application.Pipelines.Authorization;
using NArchitecture.Core.Application.Pipelines.Caching;
using NArchitecture.Core.Application.Pipelines.Logging;
using NArchitecture.Core.Application.Pipelines.Transaction;
using static Application.Features.Donors.Constants.DonorsOperationClaims;

namespace Application.Features.Donors.Commands.UpdateRange;

public class UpdateDonorRangeCommand : IRequest<UpdatedDonorRangeResponse>, ISecuredRequest, ICacheRemoverRequest, ILoggableRequest, ITransactionalRequest
{
    public ICollection<UpdateDonorRangeItem> Items { get; set; } = new List<UpdateDonorRangeItem>();

    public string[] Roles => [Admin, Write, DonorsOperationClaims.UpdateRange];

    public bool BypassCache { get; }
    public string? CacheKey { get; }
    public string[]? CacheGroupKey => ["GetDonors"];

    public class UpdateDonorRangeItem
    {
        public Guid Id { get; set; }
        public string FullName { get; set; }
        public double WeightKg { get; set; }
        public string? BloodGroup { get; set; }
        public string? Relation { get; set; }
        public string? IdentityNumber { get; set; }
    }

    public class UpdateDonorRangeCommandHandler : IRequestHandler<UpdateDonorRangeCommand, UpdatedDonorRangeResponse>
    {
        private readonly IMapper _mapper;
        private readonly IDonorRepository _donorRepository;
        private readonly DonorBusinessRules _donorBusinessRules;

        public UpdateDonorRangeCommandHandler(IMapper mapper, IDonorRepository donorRepository, DonorBusinessRules donorBusinessRules)
        {
            _mapper = mapper;
            _donorRepository = donorRepository;
            _donorBusinessRules = donorBusinessRules;
        }

        public async Task<UpdatedDonorRangeResponse> Handle(UpdateDonorRangeCommand request, CancellationToken cancellationToken)
        {
            List<Donor> items = new List<Donor>();

            foreach (UpdateDonorRangeItem item in request.Items)
            {
                Donor? entity = await _donorRepository.GetAsync(
                    predicate: x => x.Id == item.Id,
                    cancellationToken: cancellationToken
                );
                await _donorBusinessRules.DonorShouldExistWhenSelected(entity);
                _mapper.Map(source: item, destination: entity!);
                items.Add(entity!);
            }

            ICollection<Donor> updated = await _donorRepository.UpdateRangeAsync(items);

            return new UpdatedDonorRangeResponse { Ids = updated.Select(e => e.Id).ToList() };
        }
    }
}