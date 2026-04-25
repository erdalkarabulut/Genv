using System.Collections.Generic;
using NArchitecture.Core.Persistence.Paging;
using Domain.Entities;
using Microsoft.EntityFrameworkCore.Query;
using System.Linq.Expressions;

namespace Application.Services.Racks;

public interface IRackService
{
    Task<Rack?> GetAsync(
        Expression<Func<Rack, bool>> predicate,
        Func<IQueryable<Rack>, IIncludableQueryable<Rack, object>>? include = null,
        bool withDeleted = false,
        bool enableTracking = true,
        CancellationToken cancellationToken = default
    );
    Task<IPaginate<Rack>?> GetListAsync(
        Expression<Func<Rack, bool>>? predicate = null,
        Func<IQueryable<Rack>, IOrderedQueryable<Rack>>? orderBy = null,
        Func<IQueryable<Rack>, IIncludableQueryable<Rack, object>>? include = null,
        int index = 0,
        int size = 10,
        bool withDeleted = false,
        bool enableTracking = true,
        CancellationToken cancellationToken = default
    );
    Task<Rack> AddAsync(Rack rack);
    Task<ICollection<Rack>> AddRangeAsync(ICollection<Rack> racks);
    Task<Rack> UpdateAsync(Rack rack);
    Task<ICollection<Rack>> UpdateRangeAsync(ICollection<Rack> racks);
    Task<Rack> DeleteAsync(Rack rack, bool permanent = true);
    Task<ICollection<Rack>> DeleteRangeAsync(ICollection<Rack> racks, bool permanent = true);
}
