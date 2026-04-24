using Application.Features.Products.Constants;
using Application.Services.Repositories;
using AutoMapper;
using Domain.Entities;
using MediatR;
using NArchitecture.Core.Application.Pipelines.Authorization;
using NArchitecture.Core.Application.Pipelines.Caching;
using NArchitecture.Core.Application.Pipelines.Logging;
using NArchitecture.Core.Application.Pipelines.Transaction;
using static Application.Features.Products.Constants.ProductsOperationClaims;

namespace Application.Features.Products.Commands.CreateRange;

public class CreateProductRangeCommand : IRequest<CreatedProductRangeResponse>, ISecuredRequest, ICacheRemoverRequest, ILoggableRequest, ITransactionalRequest
{
    public ICollection<CreateProductRangeItem> Items { get; set; } = new List<CreateProductRangeItem>();

    public string[] Roles => [Admin, Write, ProductsOperationClaims.CreateRange];

    public bool BypassCache { get; }
    public string? CacheKey { get; }
    public string[]? CacheGroupKey => ["GetProducts"];

    public class CreateProductRangeItem
    {
        public Guid SessionId { get; set; }
        public double TotalVolume { get; set; }
        public double TotalWbc { get; set; }
        public double Cd34Percent { get; set; }
        public double Cd45Percent { get; set; }
        public double Cd3Percent { get; set; }
        public double TotalCd34PerKg { get; set; }
    }

    public class CreateProductRangeCommandHandler : IRequestHandler<CreateProductRangeCommand, CreatedProductRangeResponse>
    {
        private readonly IMapper _mapper;
        private readonly IProductRepository _productRepository;

        public CreateProductRangeCommandHandler(IMapper mapper, IProductRepository productRepository)
        {
            _mapper = mapper;
            _productRepository = productRepository;
        }

        public async Task<CreatedProductRangeResponse> Handle(CreateProductRangeCommand request, CancellationToken cancellationToken)
        {
            List<Product> products = request.Items.Select(item => _mapper.Map<Product>(item)).ToList();

            ICollection<Product> addedProducts = await _productRepository.AddRangeAsync(products);

            return new CreatedProductRangeResponse { Ids = addedProducts.Select(e => e.Id).ToList() };
        }
    }
}