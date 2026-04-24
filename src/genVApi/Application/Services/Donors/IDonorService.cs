using System.Collections.Generic;
using NArchitecture.Core.Persistence.Paging;
using Domain.Entities;
using Microsoft.EntityFrameworkCore.Query;
using System.Linq.Expressions;

namespace Application.Services.Donors;

public interface IDonorService
{
    Task<Donor?> GetAsync(
        Expression<Func<Donor, bool>> predicate,
        Func<IQueryable<Donor>, IIncludableQueryable<Donor, object>>? include = null,
        bool withDeleted = false,
        bool enableTracking = true,
        CancellationToken cancellationToken = default
    );
    Task<IPaginate<Donor>?> GetListAsync(
        Expression<Func<Donor, bool>>? predicate = null,
        Func<IQueryable<Donor>, IOrderedQueryable<Donor>>? orderBy = null,
        Func<IQueryable<Donor>, IIncludableQueryable<Donor, object>>? include = null,
        int index = 0,
        int size = 10,
        bool withDeleted = false,
        bool enableTracking = true,
        CancellationToken cancellationToken = default
    );
    Task<Donor> AddAsync(Donor donor);
    Task<ICollection<Donor>> AddRangeAsync(ICollection<Donor> donors);
    Task<Donor> UpdateAsync(Donor donor);
    Task<ICollection<Donor>> UpdateRangeAsync(ICollection<Donor> donors);
    Task<Donor> DeleteAsync(Donor donor, bool permanent = false);
    Task<ICollection<Donor>> DeleteRangeAsync(ICollection<Donor> donors, bool permanent = false);
}
