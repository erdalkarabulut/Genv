using Application.Features.Products.Constants;
using Application.Features.Products.Rules;
using Application.Services.Repositories;
using AutoMapper;
using Domain.Entities;
using MediatR;
using NArchitecture.Core.Application.Pipelines.Authorization;
using NArchitecture.Core.Application.Pipelines.Caching;
using NArchitecture.Core.Application.Pipelines.Logging;
using NArchitecture.Core.Application.Pipelines.Transaction;
using static Application.Features.Products.Constants.ProductsOperationClaims;

namespace Application.Features.Products.Commands.UpdateRange;

public class UpdateProductRangeCommand : IRequest<UpdatedProductRangeResponse>, ISecuredRequest, ICacheRemoverRequest, ILoggableRequest, ITransactionalRequest
{
    public ICollection<UpdateProductRangeItem> Items { get; set; } = new List<UpdateProductRangeItem>();

    public string[] Roles => [Admin, Write, ProductsOperationClaims.UpdateRange];

    public bool BypassCache { get; }
    public string? CacheKey { get; }
    public string[]? CacheGroupKey => ["GetProducts"];

    public class UpdateProductRangeItem
    {
        public Guid Id { get; set; }
        public Guid SessionId { get; set; }
        public double TotalVolume { get; set; }
        public double TotalWbc { get; set; }
        public double Cd34Percent { get; set; }
        public double Cd45Percent { get; set; }
        public double Cd3Percent { get; set; }
        public double TotalCd34PerKg { get; set; }
    }

    public class UpdateProductRangeCommandHandler : IRequestHandler<UpdateProductRangeCommand, UpdatedProductRangeResponse>
    {
        private readonly IMapper _mapper;
        private readonly IProductRepository _productRepository;
        private readonly ProductBusinessRules _productBusinessRules;

        public UpdateProductRangeCommandHandler(IMapper mapper, IProductRepository productRepository, ProductBusinessRules productBusinessRules)
        {
            _mapper = mapper;
            _productRepository = productRepository;
            _productBusinessRules = productBusinessRules;
        }

        public async Task<UpdatedProductRangeResponse> Handle(UpdateProductRangeCommand request, CancellationToken cancellationToken)
        {
            List<Product> items = new List<Product>();

            foreach (UpdateProductRangeItem item in request.Items)
            {
                Product? entity = await _productRepository.GetAsync(
                    predicate: x => x.Id == item.Id,
                    cancellationToken: cancellationToken
                );
                await _productBusinessRules.ProductShouldExistWhenSelected(entity);
                _mapper.Map(source: item, destination: entity!);
                items.Add(entity!);
            }

            ICollection<Product> updated = await _productRepository.UpdateRangeAsync(items);

            return new UpdatedProductRangeResponse { Ids = updated.Select(e => e.Id).ToList() };
        }
    }
}