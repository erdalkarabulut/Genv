using Application.Features.DliProducts.Constants;
using Application.Features.DliProducts.Rules;
using Application.Services.Repositories;
using Domain.Entities;
using NArchitecture.Core.Application.Pipelines.Authorization;
using NArchitecture.Core.Application.Pipelines.Caching;
using NArchitecture.Core.Application.Pipelines.Logging;
using NArchitecture.Core.Application.Pipelines.Transaction;
using MediatR;
using static Application.Features.DliProducts.Constants.DliProductsOperationClaims;

namespace Application.Features.DliProducts.Commands.DeleteRange;

public class DeleteDliProductRangeCommand : IRequest<DeletedDliProductRangeResponse>, ISecuredRequest, ICacheRemoverRequest, ILoggableRequest, ITransactionalRequest
{
    public ICollection<Guid> Ids { get; set; } = new List<Guid>();

    public string[] Roles => [Admin, Write, DliProductsOperationClaims.DeleteRange];

    public bool BypassCache { get; }
    public string? CacheKey { get; }
    public string[]? CacheGroupKey => ["GetDliProducts"];

    public class DeleteDliProductRangeCommandHandler : IRequestHandler<DeleteDliProductRangeCommand, DeletedDliProductRangeResponse>
    {
        private readonly IDliProductRepository _dliProductRepository;
        private readonly DliProductBusinessRules _dliProductBusinessRules;

        public DeleteDliProductRangeCommandHandler(IDliProductRepository dliProductRepository, DliProductBusinessRules dliProductBusinessRules)
        {
            _dliProductRepository = dliProductRepository;
            _dliProductBusinessRules = dliProductBusinessRules;
        }

        public async Task<DeletedDliProductRangeResponse> Handle(DeleteDliProductRangeCommand request, CancellationToken cancellationToken)
        {
            List<DliProduct> dliProducts = new List<DliProduct>();

            foreach (Guid id in request.Ids)
            {
                DliProduct? dliProduct = await _dliProductRepository.GetAsync(
                    predicate: dp => dp.Id == id,
                    cancellationToken: cancellationToken
                );
                await _dliProductBusinessRules.DliProductShouldExistWhenSelected(dliProduct);
                dliProducts.Add(dliProduct!);
            }

            await _dliProductRepository.DeleteRangeAsync(dliProducts);

            return new DeletedDliProductRangeResponse { DeletedCount = request.Ids.Count };
        }
    }
}
