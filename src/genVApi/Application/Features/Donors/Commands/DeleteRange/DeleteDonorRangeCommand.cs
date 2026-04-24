using Application.Features.Donors.Constants;
using Application.Features.Donors.Rules;
using Application.Services.Repositories;
using Domain.Entities;
using NArchitecture.Core.Application.Pipelines.Authorization;
using NArchitecture.Core.Application.Pipelines.Caching;
using NArchitecture.Core.Application.Pipelines.Logging;
using NArchitecture.Core.Application.Pipelines.Transaction;
using MediatR;
using static Application.Features.Donors.Constants.DonorsOperationClaims;

namespace Application.Features.Donors.Commands.DeleteRange;

public class DeleteDonorRangeCommand : IRequest<DeletedDonorRangeResponse>, ISecuredRequest, ICacheRemoverRequest, ILoggableRequest, ITransactionalRequest
{
    public ICollection<Guid> Ids { get; set; } = new List<Guid>();

    public string[] Roles => [Admin, Write, DonorsOperationClaims.DeleteRange];

    public bool BypassCache { get; }
    public string? CacheKey { get; }
    public string[]? CacheGroupKey => ["GetDonors"];

    public class DeleteDonorRangeCommandHandler : IRequestHandler<DeleteDonorRangeCommand, DeletedDonorRangeResponse>
    {
        private readonly IDonorRepository _donorRepository;
        private readonly DonorBusinessRules _donorBusinessRules;

        public DeleteDonorRangeCommandHandler(IDonorRepository donorRepository, DonorBusinessRules donorBusinessRules)
        {
            _donorRepository = donorRepository;
            _donorBusinessRules = donorBusinessRules;
        }

        public async Task<DeletedDonorRangeResponse> Handle(DeleteDonorRangeCommand request, CancellationToken cancellationToken)
        {
            List<Donor> donors = new List<Donor>();

            foreach (Guid id in request.Ids)
            {
                Donor? donor = await _donorRepository.GetAsync(
                    predicate: d => d.Id == id,
                    cancellationToken: cancellationToken
                );
                await _donorBusinessRules.DonorShouldExistWhenSelected(donor);
                donors.Add(donor!);
            }

            await _donorRepository.DeleteRangeAsync(donors);

            return new DeletedDonorRangeResponse { DeletedCount = request.Ids.Count };
        }
    }
}
