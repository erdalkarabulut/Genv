using System.Collections.Generic;
using NArchitecture.Core.Persistence.Paging;
using Domain.Entities;
using Microsoft.EntityFrameworkCore.Query;
using System.Linq.Expressions;

namespace Application.Services.BagMovements;

public interface IBagMovementService
{
    Task<BagMovement?> GetAsync(
        Expression<Func<BagMovement, bool>> predicate,
        Func<IQueryable<BagMovement>, IIncludableQueryable<BagMovement, object>>? include = null,
        bool withDeleted = false,
        bool enableTracking = true,
        CancellationToken cancellationToken = default
    );
    Task<IPaginate<BagMovement>?> GetListAsync(
        Expression<Func<BagMovement, bool>>? predicate = null,
        Func<IQueryable<BagMovement>, IOrderedQueryable<BagMovement>>? orderBy = null,
        Func<IQueryable<BagMovement>, IIncludableQueryable<BagMovement, object>>? include = null,
        int index = 0,
        int size = 10,
        bool withDeleted = false,
        bool enableTracking = true,
        CancellationToken cancellationToken = default
    );
    Task<BagMovement> AddAsync(BagMovement bagMovement);
    Task<ICollection<BagMovement>> AddRangeAsync(ICollection<BagMovement> bagMovements);
    Task<BagMovement> UpdateAsync(BagMovement bagMovement);
    Task<ICollection<BagMovement>> UpdateRangeAsync(ICollection<BagMovement> bagMovements);
    Task<BagMovement> DeleteAsync(BagMovement bagMovement, bool permanent = false);
    Task<ICollection<BagMovement>> DeleteRangeAsync(ICollection<BagMovement> bagMovements, bool permanent = false);
}
