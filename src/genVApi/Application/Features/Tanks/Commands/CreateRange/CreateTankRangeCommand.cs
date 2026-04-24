using Application.Features.Tanks.Constants;
using Application.Services.Repositories;
using AutoMapper;
using Domain.Entities;
using MediatR;
using NArchitecture.Core.Application.Pipelines.Authorization;
using NArchitecture.Core.Application.Pipelines.Caching;
using NArchitecture.Core.Application.Pipelines.Logging;
using NArchitecture.Core.Application.Pipelines.Transaction;
using static Application.Features.Tanks.Constants.TanksOperationClaims;

namespace Application.Features.Tanks.Commands.CreateRange;

public class CreateTankRangeCommand : IRequest<CreatedTankRangeResponse>, ISecuredRequest, ICacheRemoverRequest, ILoggableRequest, ITransactionalRequest
{
    public ICollection<CreateTankRangeItem> Items { get; set; } = new List<CreateTankRangeItem>();

    public string[] Roles => [Admin, Write, TanksOperationClaims.CreateRange];

    public bool BypassCache { get; }
    public string? CacheKey { get; }
    public string[]? CacheGroupKey => ["GetTanks"];

    public class CreateTankRangeItem
    {
        public string Name { get; set; }
    }

    public class CreateTankRangeCommandHandler : IRequestHandler<CreateTankRangeCommand, CreatedTankRangeResponse>
    {
        private readonly IMapper _mapper;
        private readonly ITankRepository _tankRepository;

        public CreateTankRangeCommandHandler(IMapper mapper, ITankRepository tankRepository)
        {
            _mapper = mapper;
            _tankRepository = tankRepository;
        }

        public async Task<CreatedTankRangeResponse> Handle(CreateTankRangeCommand request, CancellationToken cancellationToken)
        {
            List<Tank> tanks = request.Items.Select(item => _mapper.Map<Tank>(item)).ToList();

            ICollection<Tank> addedTanks = await _tankRepository.AddRangeAsync(tanks);

            return new CreatedTankRangeResponse { Ids = addedTanks.Select(e => e.Id).ToList() };
        }
    }
}