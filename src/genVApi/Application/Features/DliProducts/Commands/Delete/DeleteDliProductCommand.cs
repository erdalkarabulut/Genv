using Application.Features.DliProducts.Constants;
using Application.Features.DliProducts.Constants;
using Application.Features.DliProducts.Rules;
using Application.Services.Repositories;
using AutoMapper;
using Domain.Entities;
using NArchitecture.Core.Application.Pipelines.Authorization;
using NArchitecture.Core.Application.Pipelines.Caching;
using NArchitecture.Core.Application.Pipelines.Logging;
using NArchitecture.Core.Application.Pipelines.Transaction;
using MediatR;
using static Application.Features.DliProducts.Constants.DliProductsOperationClaims;

namespace Application.Features.DliProducts.Commands.Delete;

public class DeleteDliProductCommand : IRequest<DeletedDliProductResponse>, ISecuredRequest, ICacheRemoverRequest, ILoggableRequest, ITransactionalRequest
{
    public Guid Id { get; set; }

    public string[] Roles => [Admin, Write, DliProductsOperationClaims.Delete];

    public bool BypassCache { get; }
    public string? CacheKey { get; }
    public string[]? CacheGroupKey => ["GetDliProducts"];

    public class DeleteDliProductCommandHandler : IRequestHandler<DeleteDliProductCommand, DeletedDliProductResponse>
    {
        private readonly IMapper _mapper;
        private readonly IDliProductRepository _dliProductRepository;
        private readonly DliProductBusinessRules _dliProductBusinessRules;

        public DeleteDliProductCommandHandler(IMapper mapper, IDliProductRepository dliProductRepository,
                                         DliProductBusinessRules dliProductBusinessRules)
        {
            _mapper = mapper;
            _dliProductRepository = dliProductRepository;
            _dliProductBusinessRules = dliProductBusinessRules;
        }

        public async Task<DeletedDliProductResponse> Handle(DeleteDliProductCommand request, CancellationToken cancellationToken)
        {
            DliProduct? dliProduct = await _dliProductRepository.GetAsync(predicate: dp => dp.Id == request.Id, cancellationToken: cancellationToken);
            await _dliProductBusinessRules.DliProductShouldExistWhenSelected(dliProduct);

            await _dliProductRepository.DeleteAsync(dliProduct!);

            DeletedDliProductResponse response = _mapper.Map<DeletedDliProductResponse>(dliProduct);
            return response;
        }
    }
}