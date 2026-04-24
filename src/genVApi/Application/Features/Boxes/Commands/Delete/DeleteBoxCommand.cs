using Application.Features.Boxes.Constants;
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

namespace Application.Features.Boxes.Commands.Delete;

public class DeleteBoxCommand : IRequest<DeletedBoxResponse>, ISecuredRequest, ICacheRemoverRequest, ILoggableRequest, ITransactionalRequest
{
    public Guid Id { get; set; }

    public string[] Roles => [Admin, Write, BoxesOperationClaims.Delete];

    public bool BypassCache { get; }
    public string? CacheKey { get; }
    public string[]? CacheGroupKey => ["GetBoxes"];

    public class DeleteBoxCommandHandler : IRequestHandler<DeleteBoxCommand, DeletedBoxResponse>
    {
        private readonly IMapper _mapper;
        private readonly IBoxRepository _boxRepository;
        private readonly BoxBusinessRules _boxBusinessRules;

        public DeleteBoxCommandHandler(IMapper mapper, IBoxRepository boxRepository,
                                         BoxBusinessRules boxBusinessRules)
        {
            _mapper = mapper;
            _boxRepository = boxRepository;
            _boxBusinessRules = boxBusinessRules;
        }

        public async Task<DeletedBoxResponse> Handle(DeleteBoxCommand request, CancellationToken cancellationToken)
        {
            Box? box = await _boxRepository.GetAsync(predicate: b => b.Id == request.Id, cancellationToken: cancellationToken);
            await _boxBusinessRules.BoxShouldExistWhenSelected(box);

            await _boxRepository.DeleteAsync(box!);

            DeletedBoxResponse response = _mapper.Map<DeletedBoxResponse>(box);
            return response;
        }
    }
}