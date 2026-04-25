using System.Collections.Generic;
using NArchitecture.Core.Persistence.Paging;
using Domain.Entities;
using Microsoft.EntityFrameworkCore.Query;
using System.Linq.Expressions;

namespace Application.Services.DliProducts;

public interface IDliProductService
{
    Task<DliProduct?> GetAsync(
        Expression<Func<DliProduct, bool>> predicate,
        Func<IQueryable<DliProduct>, IIncludableQueryable<DliProduct, object>>? include = null,
        bool withDeleted = false,
        bool enableTracking = true,
        CancellationToken cancellationToken = default
    );
    Task<IPaginate<DliProduct>?> GetListAsync(
        Expression<Func<DliProduct, bool>>? predicate = null,
        Func<IQueryable<DliProduct>, IOrderedQueryable<DliProduct>>? orderBy = null,
        Func<IQueryable<DliProduct>, IIncludableQueryable<DliProduct, object>>? include = null,
        int index = 0,
        int size = 10,
        bool withDeleted = false,
        bool enableTracking = true,
        CancellationToken cancellationToken = default
    );
    Task<DliProduct> AddAsync(DliProduct dliProduct);
    Task<ICollection<DliProduct>> AddRangeAsync(ICollection<DliProduct> dliProducts);
    Task<DliProduct> UpdateAsync(DliProduct dliProduct);
    Task<ICollection<DliProduct>> UpdateRangeAsync(ICollection<DliProduct> dliProducts);
    Task<DliProduct> DeleteAsync(DliProduct dliProduct, bool permanent = true);
    Task<ICollection<DliProduct>> DeleteRangeAsync(ICollection<DliProduct> dliProducts, bool permanent = true);
}
