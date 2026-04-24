using System.Collections.Generic;
using NArchitecture.Core.Persistence.Paging;
using Domain.Entities;
using Microsoft.EntityFrameworkCore.Query;
using System.Linq.Expressions;

namespace Application.Services.Boxes;

public interface IBoxService
{
    Task<Box?> GetAsync(
        Expression<Func<Box, bool>> predicate,
        Func<IQueryable<Box>, IIncludableQueryable<Box, object>>? include = null,
        bool withDeleted = false,
        bool enableTracking = true,
        CancellationToken cancellationToken = default
    );
    Task<IPaginate<Box>?> GetListAsync(
        Expression<Func<Box, bool>>? predicate = null,
        Func<IQueryable<Box>, IOrderedQueryable<Box>>? orderBy = null,
        Func<IQueryable<Box>, IIncludableQueryable<Box, object>>? include = null,
        int index = 0,
        int size = 10,
        bool withDeleted = false,
        bool enableTracking = true,
        CancellationToken cancellationToken = default
    );
    Task<Box> AddAsync(Box box);
    Task<ICollection<Box>> AddRangeAsync(ICollection<Box> boxs);
    Task<Box> UpdateAsync(Box box);
    Task<ICollection<Box>> UpdateRangeAsync(ICollection<Box> boxs);
    Task<Box> DeleteAsync(Box box, bool permanent = false);
    Task<ICollection<Box>> DeleteRangeAsync(ICollection<Box> boxs, bool permanent = false);
}
