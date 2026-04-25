using Application.Features.Bags.Constants;
using Application.Services.Repositories;
using AutoMapper;
using Domain.Entities;
using MediatR;
using NArchitecture.Core.Application.Pipelines.Authorization;
using NArchitecture.Core.Application.Pipelines.Caching;
using NArchitecture.Core.Application.Pipelines.Logging;
using NArchitecture.Core.Application.Pipelines.Transaction;
using static Application.Features.Bags.Constants.BagsOperationClaims;

namespace Application.Features.Bags.Commands.CreateRange;

public class CreateBagRangeCommand : IRequest<CreatedBagRangeResponse>, ISecuredRequest, ICacheRemoverRequest, ILoggableRequest, ITransactionalRequest
{
    public ICollection<CreateBagRangeItem> Items { get; set; } = new List<CreateBagRangeItem>();

    public string[] Roles => [Admin, Write, BagsOperationClaims.CreateRange];

    public bool BypassCache { get; }
    public string? CacheKey { get; }
    public string[]? CacheGroupKey => ["GetBags"];

    public class CreateBagRangeItem
    {
        public Guid SessionId { get; set; }
        public int BagNumber { get; set; }
        public double VolumeMl { get; set; }
        public double SourceVolumeMl { get; set; }
        public double Cd34PerKg { get; set; }
        public double Cd3PerKg { get; set; }
        public Domain.Enums.BagStatus Status { get; set; }
        public Domain.Enums.BagPurpose Purpose { get; set; } = Domain.Enums.BagPurpose.Cryo;
        public Guid? SplitBatchId { get; set; }
        public Guid? BagCellId { get; set; }
    }

    public class CreateBagRangeCommandHandler : IRequestHandler<CreateBagRangeCommand, CreatedBagRangeResponse>
    {
        private readonly IMapper _mapper;
        private readonly IBagRepository _bagRepository;

        public CreateBagRangeCommandHandler(IMapper mapper, IBagRepository bagRepository)
        {
            _mapper = mapper;
            _bagRepository = bagRepository;
        }

        public async Task<CreatedBagRangeResponse> Handle(CreateBagRangeCommand request, CancellationToken cancellationToken)
        {
            List<Bag> bags = request.Items.Select(item => _mapper.Map<Bag>(item)).ToList();

            ICollection<Bag> addedBags = await _bagRepository.AddRangeAsync(bags);

            return new CreatedBagRangeResponse { Ids = addedBags.Select(e => e.Id).ToList() };
        }
    }
}