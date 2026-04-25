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

namespace Application.Features.Boxes.Commands.Update;

public class UpdateBoxCommand : IRequest<UpdatedBoxResponse>, ISecuredRequest, ICacheRemoverRequest, ILoggableRequest, ITransactionalRequest
{
    public Guid Id { get; set; }
    public Guid SlotId { get; set; }
    public string Name { get; set; }

    public string[] Roles => [Admin, Write, BoxesOperationClaims.Update];

    public bool BypassCache { get; }
    public string? CacheKey { get; }
    public string[]? CacheGroupKey => ["GetBoxes"];

    public class UpdateBoxCommandHandler : IRequestHandler<UpdateBoxCommand, UpdatedBoxResponse>
    {
        private readonly IMapper _mapper;
        private readonly IBoxRepository _boxRepository;
        private readonly BoxBusinessRules _boxBusinessRules;

        public UpdateBoxCommandHandler(IMapper mapper, IBoxRepository boxRepository,
                                         BoxBusinessRules boxBusinessRules)
        {
            _mapper = mapper;
            _boxRepository = boxRepository;
            _boxBusinessRules = boxBusinessRules;
        }

        public async Task<UpdatedBoxResponse> Handle(UpdateBoxCommand request, CancellationToken cancellationToken)
        {
            Box? box = await _boxRepository.GetAsync(predicate: x => x.Id == request.Id, cancellationToken: cancellationToken);
            await _boxBusinessRules.BoxShouldExistWhenSelected(box);
            box = _mapper.Map(request, box);

            await _boxRepository.UpdateAsync(box!);

            UpdatedBoxResponse response = _mapper.Map<UpdatedBoxResponse>(box);
            return response;
        }
    }
}