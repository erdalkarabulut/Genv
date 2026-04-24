using Application.Features.Boxes.Constants;
using Application.Services.Repositories;
using AutoMapper;
using Domain.Entities;
using MediatR;
using NArchitecture.Core.Application.Pipelines.Authorization;
using NArchitecture.Core.Application.Pipelines.Caching;
using NArchitecture.Core.Application.Pipelines.Logging;
using NArchitecture.Core.Application.Pipelines.Transaction;
using static Application.Features.Boxes.Constants.BoxesOperationClaims;

namespace Application.Features.Boxes.Commands.CreateRange;

public class CreateBoxRangeCommand : IRequest<CreatedBoxRangeResponse>, ISecuredRequest, ICacheRemoverRequest, ILoggableRequest, ITransactionalRequest
{
    public ICollection<CreateBoxRangeItem> Items { get; set; } = new List<CreateBoxRangeItem>();

    public string[] Roles => [Admin, Write, BoxesOperationClaims.CreateRange];

    public bool BypassCache { get; }
    public string? CacheKey { get; }
    public string[]? CacheGroupKey => ["GetBoxes"];

    public class CreateBoxRangeItem
    {
        public Guid RackId { get; set; }
        public string Name { get; set; }
    }

    public class CreateBoxRangeCommandHandler : IRequestHandler<CreateBoxRangeCommand, CreatedBoxRangeResponse>
    {
        private readonly IMapper _mapper;
        private readonly IBoxRepository _boxRepository;

        public CreateBoxRangeCommandHandler(IMapper mapper, IBoxRepository boxRepository)
        {
            _mapper = mapper;
            _boxRepository = boxRepository;
        }

        public async Task<CreatedBoxRangeResponse> Handle(CreateBoxRangeCommand request, CancellationToken cancellationToken)
        {
            List<Box> boxs = request.Items.Select(item => _mapper.Map<Box>(item)).ToList();

            ICollection<Box> addedBoxs = await _boxRepository.AddRangeAsync(boxs);

            return new CreatedBoxRangeResponse { Ids = addedBoxs.Select(e => e.Id).ToList() };
        }
    }
}