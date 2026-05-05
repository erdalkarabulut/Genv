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
using NArchitecture.Core.CrossCuttingConcerns.Exception.Types;
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
        private readonly IBagCellRepository _bagCellRepository;
        private readonly BoxBusinessRules _boxBusinessRules;

        public DeleteBoxCommandHandler(IMapper mapper, IBoxRepository boxRepository,
                                         IBagCellRepository bagCellRepository,
                                         BoxBusinessRules boxBusinessRules)
        {
            _mapper = mapper;
            _boxRepository = boxRepository;
            _bagCellRepository = bagCellRepository;
            _boxBusinessRules = boxBusinessRules;
        }

        public async Task<DeletedBoxResponse> Handle(DeleteBoxCommand request, CancellationToken cancellationToken)
        {
            Box? box = await _boxRepository.GetAsync(predicate: b => b.Id == request.Id, cancellationToken: cancellationToken);
            await _boxBusinessRules.BoxShouldExistWhenSelected(box);

            bool hasCells = await _bagCellRepository.AnyAsync(
                predicate: c => c.BoxId == request.Id,
                enableTracking: false,
                cancellationToken: cancellationToken);
            if (hasCells)
                throw new BusinessException("Bu kutuda hücreler var. Önce hücreleri silin.");

            await _boxRepository.DeleteAsync(box!, permanent: true);

            DeletedBoxResponse response = _mapper.Map<DeletedBoxResponse>(box);
            return response;
        }
    }
}