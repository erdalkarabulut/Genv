using System.Collections.Generic;
using Application.Features.Products.Rules;
using Application.Services.Repositories;
using NArchitecture.Core.Persistence.Paging;
using Domain.Entities;
using Microsoft.EntityFrameworkCore.Query;
using System.Linq.Expressions;

namespace Application.Services.Products;

public class ProductManager : IProductService
{
    private readonly IProductRepository _productRepository;
    private readonly ProductBusinessRules _productBusinessRules;

    public ProductManager(IProductRepository productRepository, ProductBusinessRules productBusinessRules)
    {
        _productRepository = productRepository;
        _productBusinessRules = productBusinessRules;
    }

    public async Task<Product?> GetAsync(
        Expression<Func<Product, bool>> predicate,
        Func<IQueryable<Product>, IIncludableQueryable<Product, object>>? include = null,
        bool withDeleted = false,
        bool enableTracking = true,
        CancellationToken cancellationToken = default
    )
    {
        Product? product = await _productRepository.GetAsync(predicate, include, withDeleted, enableTracking, cancellationToken);
        return product;
    }

    public async Task<IPaginate<Product>?> GetListAsync(
        Expression<Func<Product, bool>>? predicate = null,
        Func<IQueryable<Product>, IOrderedQueryable<Product>>? orderBy = null,
        Func<IQueryable<Product>, IIncludableQueryable<Product, object>>? include = null,
        int index = 0,
        int size = 10,
        bool withDeleted = false,
        bool enableTracking = true,
        CancellationToken cancellationToken = default
    )
    {
        IPaginate<Product> productList = await _productRepository.GetListAsync(
            predicate,
            orderBy,
            include,
            index,
            size,
            withDeleted,
            enableTracking,
            cancellationToken
        );
        return productList;
    }

    public async Task<Product> AddAsync(Product product)
    {
        Product addedProduct = await _productRepository.AddAsync(product);

        return addedProduct;
    }

    public async Task<ICollection<Product>> AddRangeAsync(ICollection<Product> products)
    {
        ICollection<Product> addedProducts = await _productRepository.AddRangeAsync(products);

        return addedProducts;
    }

    public async Task<Product> UpdateAsync(Product product)
    {
        Product updatedProduct = await _productRepository.UpdateAsync(product);

        return updatedProduct;
    }

    public async Task<ICollection<Product>> UpdateRangeAsync(ICollection<Product> products)
    {
        ICollection<Product> updatedProducts = await _productRepository.UpdateRangeAsync(products);

        return updatedProducts;
    }

    public async Task<Product> DeleteAsync(Product product, bool permanent = false)
    {
        Product deletedProduct = await _productRepository.DeleteAsync(product);

        return deletedProduct;
    }

    public async Task<ICollection<Product>> DeleteRangeAsync(ICollection<Product> products, bool permanent = false)
    {
        ICollection<Product> deletedProducts = await _productRepository.DeleteRangeAsync(products, permanent);

        return deletedProducts;
    }
}
