using Application.Features.Bags.Constants;
using Application.Features.Bags.Rules;
using Application.Services.Repositories;
using AutoMapper;
using Domain.Entities;
using NArchitecture.Core.Application.Pipelines.Authorization;
using NArchitecture.Core.Application.Pipelines.Caching;
using NArchitecture.Core.Application.Pipelines.Logging;
using NArchitecture.Core.Application.Pipelines.Transaction;
using MediatR;
using static Application.Features.Bags.Constants.BagsOperationClaims;

namespace Application.Features.Bags.Commands.Update;

public class UpdateBagCommand : IRequest<UpdatedBagResponse>, ISecuredRequest, ICacheRemoverRequest, ILoggableRequest, ITransactionalRequest
{
    public Guid Id { get; set; }
    public Guid SessionId { get; set; }
    public int BagNumber { get; set; }
    public double VolumeMl { get; set; }
    public double SourceVolumeMl { get; set; }
    public double? Wbc { get; set; }
    public double? Cd34Percent { get; set; }
    public double? Cd45Percent { get; set; }
    public double? Cd3Percent { get; set; }
    public string? CompositionNote { get; set; }
    public double Cd34PerKg { get; set; }
    public double Cd3PerKg { get; set; }
    public Domain.Enums.BagStatus Status { get; set; }
    public Domain.Enums.BagPurpose Purpose { get; set; }
    public Guid? SplitBatchId { get; set; }
    public Guid? SlotId { get; set; }

    public string[] Roles => [Admin, Write, BagsOperationClaims.Update];

    public bool BypassCache { get; }
    public string? CacheKey { get; }
    public string[]? CacheGroupKey => ["GetBags"];

    public class UpdateBagCommandHandler : IRequestHandler<UpdateBagCommand, UpdatedBagResponse>
    {
        private readonly IMapper _mapper;
        private readonly IBagRepository _bagRepository;
        private readonly BagBusinessRules _bagBusinessRules;

        public UpdateBagCommandHandler(IMapper mapper, IBagRepository bagRepository,
                                         BagBusinessRules bagBusinessRules)
        {
            _mapper = mapper;
            _bagRepository = bagRepository;
            _bagBusinessRules = bagBusinessRules;
        }

        public async Task<UpdatedBagResponse> Handle(UpdateBagCommand request, CancellationToken cancellationToken)
        {
            Bag? bag = await _bagRepository.GetAsync(predicate: x => x.Id == request.Id, cancellationToken: cancellationToken);
            await _bagBusinessRules.BagShouldExistWhenSelected(bag);
            bag = _mapper.Map(request, bag);

            await _bagRepository.UpdateAsync(bag!);

            UpdatedBagResponse response = _mapper.Map<UpdatedBagResponse>(bag);
            return response;
        }
    }
}