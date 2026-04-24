using System.Collections.Generic;
using NArchitecture.Core.Persistence.Paging;
using Domain.Entities;
using Microsoft.EntityFrameworkCore.Query;
using System.Linq.Expressions;

namespace Application.Services.Products;

public interface IProductService
{
    Task<Product?> GetAsync(
        Expression<Func<Product, bool>> predicate,
        Func<IQueryable<Product>, IIncludableQueryable<Product, object>>? include = null,
        bool withDeleted = false,
        bool enableTracking = true,
        CancellationToken cancellationToken = default
    );
    Task<IPaginate<Product>?> GetListAsync(
        Expression<Func<Product, bool>>? predicate = null,
        Func<IQueryable<Product>, IOrderedQueryable<Product>>? orderBy = null,
        Func<IQueryable<Product>, IIncludableQueryable<Product, object>>? include = null,
        int index = 0,
        int size = 10,
        bool withDeleted = false,
        bool enableTracking = true,
        CancellationToken cancellationToken = default
    );
    Task<Product> AddAsync(Product product);
    Task<ICollection<Product>> AddRangeAsync(ICollection<Product> products);
    Task<Product> UpdateAsync(Product product);
    Task<ICollection<Product>> UpdateRangeAsync(ICollection<Product> products);
    Task<Product> DeleteAsync(Product product, bool permanent = false);
    Task<ICollection<Product>> DeleteRangeAsync(ICollection<Product> products, bool permanent = false);
}
