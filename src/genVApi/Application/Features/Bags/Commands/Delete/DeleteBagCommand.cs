using Application.Features.Bags.Constants;
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

namespace Application.Features.Bags.Commands.Delete;

public class DeleteBagCommand : IRequest<DeletedBagResponse>, ISecuredRequest, ICacheRemoverRequest, ILoggableRequest, ITransactionalRequest
{
    public Guid Id { get; set; }

    public string[] Roles => [Admin, Write, BagsOperationClaims.Delete];

    public bool BypassCache { get; }
    public string? CacheKey { get; }
    public string[]? CacheGroupKey => ["GetBags"];

    public class DeleteBagCommandHandler : IRequestHandler<DeleteBagCommand, DeletedBagResponse>
    {
        private readonly IMapper _mapper;
        private readonly IBagRepository _bagRepository;
        private readonly BagBusinessRules _bagBusinessRules;

        public DeleteBagCommandHandler(IMapper mapper, IBagRepository bagRepository,
                                         BagBusinessRules bagBusinessRules)
        {
            _mapper = mapper;
            _bagRepository = bagRepository;
            _bagBusinessRules = bagBusinessRules;
        }

        public async Task<DeletedBagResponse> Handle(DeleteBagCommand request, CancellationToken cancellationToken)
        {
            Bag? bag = await _bagRepository.GetAsync(predicate: b => b.Id == request.Id, cancellationToken: cancellationToken);
            await _bagBusinessRules.BagShouldExistWhenSelected(bag);

            await _bagRepository.DeleteAsync(bag!);

            DeletedBagResponse response = _mapper.Map<DeletedBagResponse>(bag);
            return response;
        }
    }
}