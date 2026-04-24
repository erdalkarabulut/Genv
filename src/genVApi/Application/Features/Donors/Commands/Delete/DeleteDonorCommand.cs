using Application.Features.Donors.Constants;
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

namespace Application.Features.Donors.Commands.Delete;

public class DeleteDonorCommand : IRequest<DeletedDonorResponse>, ISecuredRequest, ICacheRemoverRequest, ILoggableRequest, ITransactionalRequest
{
    public Guid Id { get; set; }

    public string[] Roles => [Admin, Write, DonorsOperationClaims.Delete];

    public bool BypassCache { get; }
    public string? CacheKey { get; }
    public string[]? CacheGroupKey => ["GetDonors"];

    public class DeleteDonorCommandHandler : IRequestHandler<DeleteDonorCommand, DeletedDonorResponse>
    {
        private readonly IMapper _mapper;
        private readonly IDonorRepository _donorRepository;
        private readonly DonorBusinessRules _donorBusinessRules;

        public DeleteDonorCommandHandler(IMapper mapper, IDonorRepository donorRepository,
                                         DonorBusinessRules donorBusinessRules)
        {
            _mapper = mapper;
            _donorRepository = donorRepository;
            _donorBusinessRules = donorBusinessRules;
        }

        public async Task<DeletedDonorResponse> Handle(DeleteDonorCommand request, CancellationToken cancellationToken)
        {
            Donor? donor = await _donorRepository.GetAsync(predicate: d => d.Id == request.Id, cancellationToken: cancellationToken);
            await _donorBusinessRules.DonorShouldExistWhenSelected(donor);

            await _donorRepository.DeleteAsync(donor!);

            DeletedDonorResponse response = _mapper.Map<DeletedDonorResponse>(donor);
            return response;
        }
    }
}