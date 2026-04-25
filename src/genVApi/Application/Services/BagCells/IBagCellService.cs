using System.Collections.Generic;
using System.Linq.Expressions;
using Domain.Entities;
using Microsoft.EntityFrameworkCore.Query;
using NArchitecture.Core.Persistence.Paging;

namespace Application.Services.BagCells;

public interface IBagCellService
{
    Task<BagCell?> GetAsync(
        Expression<Func<BagCell, bool>> predicate,
        Func<IQueryable<BagCell>, IIncludableQueryable<BagCell, object>>? include = null,
        bool withDeleted = false,
        bool enableTracking = true,
        CancellationToken cancellationToken = default
    );

    Task<IPaginate<BagCell>?> GetListAsync(
        Expression<Func<BagCell, bool>>? predicate = null,
        Func<IQueryable<BagCell>, IOrderedQueryable<BagCell>>? orderBy = null,
        Func<IQueryable<BagCell>, IIncludableQueryable<BagCell, object>>? include = null,
        int index = 0,
        int size = 10,
        bool withDeleted = false,
        bool enableTracking = true,
        CancellationToken cancellationToken = default
    );

    Task<BagCell> AddAsync(BagCell bagCell);
    Task<ICollection<BagCell>> AddRangeAsync(ICollection<BagCell> bagCells);
    Task<BagCell> UpdateAsync(BagCell bagCell);
    Task<ICollection<BagCell>> UpdateRangeAsync(ICollection<BagCell> bagCells);
    Task<BagCell> DeleteAsync(BagCell bagCell, bool permanent = true);
    Task<ICollection<BagCell>> DeleteRangeAsync(ICollection<BagCell> bagCells, bool permanent = true);
}
