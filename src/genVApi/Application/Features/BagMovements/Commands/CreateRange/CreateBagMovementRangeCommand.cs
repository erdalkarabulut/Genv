using Application.Features.BagMovements.Constants;
using Application.Services.Repositories;
using AutoMapper;
using Domain.Entities;
using MediatR;
using NArchitecture.Core.Application.Pipelines.Authorization;
using NArchitecture.Core.Application.Pipelines.Caching;
using NArchitecture.Core.Application.Pipelines.Logging;
using NArchitecture.Core.Application.Pipelines.Transaction;
using static Application.Features.BagMovements.Constants.BagMovementsOperationClaims;

namespace Application.Features.BagMovements.Commands.CreateRange;

public class CreateBagMovementRangeCommand : IRequest<CreatedBagMovementRangeResponse>, ISecuredRequest, ICacheRemoverRequest, ILoggableRequest, ITransactionalRequest
{
    public ICollection<CreateBagMovementRangeItem> Items { get; set; } = new List<CreateBagMovementRangeItem>();

    public string[] Roles => [Admin, Write, BagMovementsOperationClaims.CreateRange];

    public bool BypassCache { get; }
    public string? CacheKey { get; }
    public string[]? CacheGroupKey => ["GetBagMovements"];

    public class CreateBagMovementRangeItem
    {
        public Guid BagId { get; set; }
        public Guid? FromSlotId { get; set; }
        public Guid? ToSlotId { get; set; }
        public string Action { get; set; }
    }

    public class CreateBagMovementRangeCommandHandler : IRequestHandler<CreateBagMovementRangeCommand, CreatedBagMovementRangeResponse>
    {
        private readonly IMapper _mapper;
        private readonly IBagMovementRepository _bagMovementRepository;

        public CreateBagMovementRangeCommandHandler(IMapper mapper, IBagMovementRepository bagMovementRepository)
        {
            _mapper = mapper;
            _bagMovementRepository = bagMovementRepository;
        }

        public async Task<CreatedBagMovementRangeResponse> Handle(CreateBagMovementRangeCommand request, CancellationToken cancellationToken)
        {
            List<BagMovement> bagMovements = request.Items.Select(item => _mapper.Map<BagMovement>(item)).ToList();

            ICollection<BagMovement> addedBagMovements = await _bagMovementRepository.AddRangeAsync(bagMovements);

            return new CreatedBagMovementRangeResponse { Ids = addedBagMovements.Select(e => e.Id).ToList() };
        }
    }
}