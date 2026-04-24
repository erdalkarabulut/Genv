using System.Collections.Generic;
using NArchitecture.Core.Persistence.Paging;
using Domain.Entities;
using Microsoft.EntityFrameworkCore.Query;
using System.Linq.Expressions;

namespace Application.Services.Slots;

public interface ISlotService
{
    Task<Slot?> GetAsync(
        Expression<Func<Slot, bool>> predicate,
        Func<IQueryable<Slot>, IIncludableQueryable<Slot, object>>? include = null,
        bool withDeleted = false,
        bool enableTracking = true,
        CancellationToken cancellationToken = default
    );
    Task<IPaginate<Slot>?> GetListAsync(
        Expression<Func<Slot, bool>>? predicate = null,
        Func<IQueryable<Slot>, IOrderedQueryable<Slot>>? orderBy = null,
        Func<IQueryable<Slot>, IIncludableQueryable<Slot, object>>? include = null,
        int index = 0,
        int size = 10,
        bool withDeleted = false,
        bool enableTracking = true,
        CancellationToken cancellationToken = default
    );
    Task<Slot> AddAsync(Slot slot);
    Task<ICollection<Slot>> AddRangeAsync(ICollection<Slot> slots);
    Task<Slot> UpdateAsync(Slot slot);
    Task<ICollection<Slot>> UpdateRangeAsync(ICollection<Slot> slots);
    Task<Slot> DeleteAsync(Slot slot, bool permanent = false);
    Task<ICollection<Slot>> DeleteRangeAsync(ICollection<Slot> slots, bool permanent = false);
}
