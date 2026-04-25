using System.Collections.Generic;
using NArchitecture.Core.Persistence.Paging;
using Domain.Entities;
using Microsoft.EntityFrameworkCore.Query;
using System.Linq.Expressions;

namespace Application.Services.Bags;

public interface IBagService
{
    Task<Bag?> GetAsync(
        Expression<Func<Bag, bool>> predicate,
        Func<IQueryable<Bag>, IIncludableQueryable<Bag, object>>? include = null,
        bool withDeleted = false,
        bool enableTracking = true,
        CancellationToken cancellationToken = default
    );
    Task<IPaginate<Bag>?> GetListAsync(
        Expression<Func<Bag, bool>>? predicate = null,
        Func<IQueryable<Bag>, IOrderedQueryable<Bag>>? orderBy = null,
        Func<IQueryable<Bag>, IIncludableQueryable<Bag, object>>? include = null,
        int index = 0,
        int size = 10,
        bool withDeleted = false,
        bool enableTracking = true,
        CancellationToken cancellationToken = default
    );
    Task<Bag> AddAsync(Bag bag);
    Task<ICollection<Bag>> AddRangeAsync(ICollection<Bag> bags);
    Task<Bag> UpdateAsync(Bag bag);
    Task<ICollection<Bag>> UpdateRangeAsync(ICollection<Bag> bags);
    Task<Bag> DeleteAsync(Bag bag, bool permanent = true);
    Task<ICollection<Bag>> DeleteRangeAsync(ICollection<Bag> bags, bool permanent = true);
}
