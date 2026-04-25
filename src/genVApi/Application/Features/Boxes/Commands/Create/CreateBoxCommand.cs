using Application.Features.Boxes.Constants;
using Application.Features.Boxes.Rules;
using Application.Services.Repositories;
using AutoMapper;
using Domain.Entities;
using NArchitecture.Core.Application.Pipelines.Authorization;
using NArchitecture.Core.Application.Pipelines.Caching;
using NArchitecture.Core.Application.Pipelines.Logging;
using NArchitecture.Core.Application.Pipelines.Transaction;
using MediatR;
using static Application.Features.Boxes.Constants.BoxesOperationClaims;

namespace Application.Features.Boxes.Commands.Create;

public class CreateBoxCommand : IRequest<CreatedBoxResponse>, ISecuredRequest, ICacheRemoverRequest, ILoggableRequest, ITransactionalRequest
{
    /// <summary>Raf üzeri slot (Tank → Rack → Slot).</summary>
    public Guid SlotId { get; set; }
    public string Name { get; set; }

    public string[] Roles => [Admin, Write, BoxesOperationClaims.Create];

    public bool BypassCache { get; }
    public string? CacheKey { get; }
    public string[]? CacheGroupKey => ["GetBoxes", "GetCryoGrid", "GetRackSlots"];

    public class CreateBoxCommandHandler : IRequestHandler<CreateBoxCommand, CreatedBoxResponse>
    {
        private readonly IMapper _mapper;
        private readonly IBoxRepository _boxRepository;
        private readonly BoxBusinessRules _boxBusinessRules;

        public CreateBoxCommandHandler(IMapper mapper, IBoxRepository boxRepository,
                                         BoxBusinessRules boxBusinessRules)
        {
            _mapper = mapper;
            _boxRepository = boxRepository;
            _boxBusinessRules = boxBusinessRules;
        }

        public async Task<CreatedBoxResponse> Handle(CreateBoxCommand request, CancellationToken cancellationToken)
        {
            Box box = _mapper.Map<Box>(request);

            await _boxRepository.AddAsync(box);

            CreatedBoxResponse response = _mapper.Map<CreatedBoxResponse>(box);
            return response;
        }
    }
}