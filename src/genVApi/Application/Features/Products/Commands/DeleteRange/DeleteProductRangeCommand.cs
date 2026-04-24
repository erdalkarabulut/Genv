using Application.Features.Products.Constants;
using Application.Features.Products.Rules;
using Application.Services.Repositories;
using Domain.Entities;
using NArchitecture.Core.Application.Pipelines.Authorization;
using NArchitecture.Core.Application.Pipelines.Caching;
using NArchitecture.Core.Application.Pipelines.Logging;
using NArchitecture.Core.Application.Pipelines.Transaction;
using MediatR;
using static Application.Features.Products.Constants.ProductsOperationClaims;

namespace Application.Features.Products.Commands.DeleteRange;

public class DeleteProductRangeCommand : IRequest<DeletedProductRangeResponse>, ISecuredRequest, ICacheRemoverRequest, ILoggableRequest, ITransactionalRequest
{
    public ICollection<Guid> Ids { get; set; } = new List<Guid>();

    public string[] Roles => [Admin, Write, ProductsOperationClaims.DeleteRange];

    public bool BypassCache { get; }
    public string? CacheKey { get; }
    public string[]? CacheGroupKey => ["GetProducts"];

    public class DeleteProductRangeCommandHandler : IRequestHandler<DeleteProductRangeCommand, DeletedProductRangeResponse>
    {
        private readonly IProductRepository _productRepository;
        private readonly ProductBusinessRules _productBusinessRules;

        public DeleteProductRangeCommandHandler(IProductRepository productRepository, ProductBusinessRules productBusinessRules)
        {
            _productRepository = productRepository;
            _productBusinessRules = productBusinessRules;
        }

        public async Task<DeletedProductRangeResponse> Handle(DeleteProductRangeCommand request, CancellationToken cancellationToken)
        {
            List<Product> products = new List<Product>();

            foreach (Guid id in request.Ids)
            {
                Product? product = await _productRepository.GetAsync(
                    predicate: p => p.Id == id,
                    cancellationToken: cancellationToken
                );
                await _productBusinessRules.ProductShouldExistWhenSelected(product);
                products.Add(product!);
            }

            await _productRepository.DeleteRangeAsync(products);

            return new DeletedProductRangeResponse { DeletedCount = request.Ids.Count };
        }
    }
}
