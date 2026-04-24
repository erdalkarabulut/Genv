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

namespace Application.Features.Bags.Commands.Create;

public class CreateBagCommand : IRequest<CreatedBagResponse>, ISecuredRequest, ICacheRemoverRequest, ILoggableRequest, ITransactionalRequest
{
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
    public Domain.Enums.BagPurpose Purpose { get; set; } = Domain.Enums.BagPurpose.Cryo;
    public Guid? SplitBatchId { get; set; }
    public Guid? SlotId { get; set; }

    public string[] Roles => [Admin, Write, BagsOperationClaims.Create];

    public bool BypassCache { get; }
    public string? CacheKey { get; }
    public string[]? CacheGroupKey => ["GetBags"];

    public class CreateBagCommandHandler : IRequestHandler<CreateBagCommand, CreatedBagResponse>
    {
        private readonly IMapper _mapper;
        private readonly IBagRepository _bagRepository;
        private readonly BagBusinessRules _bagBusinessRules;

        public CreateBagCommandHandler(IMapper mapper, IBagRepository bagRepository,
                                         BagBusinessRules bagBusinessRules)
        {
            _mapper = mapper;
            _bagRepository = bagRepository;
            _bagBusinessRules = bagBusinessRules;
        }

        public async Task<CreatedBagResponse> Handle(CreateBagCommand request, CancellationToken cancellationToken)
        {
            Bag bag = _mapper.Map<Bag>(request);

            await _bagRepository.AddAsync(bag);

            CreatedBagResponse response = _mapper.Map<CreatedBagResponse>(bag);
            return response;
        }
    }
}