using System.Collections.Generic;
using NArchitecture.Core.Persistence.Paging;
using Domain.Entities;
using Microsoft.EntityFrameworkCore.Query;
using System.Linq.Expressions;

namespace Application.Services.Tanks;

public interface ITankService
{
    Task<Tank?> GetAsync(
        Expression<Func<Tank, bool>> predicate,
        Func<IQueryable<Tank>, IIncludableQueryable<Tank, object>>? include = null,
        bool withDeleted = false,
        bool enableTracking = true,
        CancellationToken cancellationToken = default
    );
    Task<IPaginate<Tank>?> GetListAsync(
        Expression<Func<Tank, bool>>? predicate = null,
        Func<IQueryable<Tank>, IOrderedQueryable<Tank>>? orderBy = null,
        Func<IQueryable<Tank>, IIncludableQueryable<Tank, object>>? include = null,
        int index = 0,
        int size = 10,
        bool withDeleted = false,
        bool enableTracking = true,
        CancellationToken cancellationToken = default
    );
    Task<Tank> AddAsync(Tank tank);
    Task<ICollection<Tank>> AddRangeAsync(ICollection<Tank> tanks);
    Task<Tank> UpdateAsync(Tank tank);
    Task<ICollection<Tank>> UpdateRangeAsync(ICollection<Tank> tanks);
    Task<Tank> DeleteAsync(Tank tank, bool permanent = true);
    Task<ICollection<Tank>> DeleteRangeAsync(ICollection<Tank> tanks, bool permanent = true);
}
