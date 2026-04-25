using System.Collections.Generic;
using System.Linq.Expressions;
using Application.Services.Repositories;
using Domain.Entities;
using Microsoft.EntityFrameworkCore.Query;
using NArchitecture.Core.Persistence.Paging;

namespace Application.Services.BagCells;

public class BagCellManager : IBagCellService
{
    private readonly IBagCellRepository _bagCellRepository;

    public BagCellManager(IBagCellRepository bagCellRepository)
    {
        _bagCellRepository = bagCellRepository;
    }

    public async Task<BagCell?> GetAsync(
        Expression<Func<BagCell, bool>> predicate,
        Func<IQueryable<BagCell>, IIncludableQueryable<BagCell, object>>? include = null,
        bool withDeleted = false,
        bool enableTracking = true,
        CancellationToken cancellationToken = default
    ) =>
        await _bagCellRepository.GetAsync(predicate, include, withDeleted, enableTracking, cancellationToken);

    public async Task<IPaginate<BagCell>?> GetListAsync(
        Expression<Func<BagCell, bool>>? predicate = null,
        Func<IQueryable<BagCell>, IOrderedQueryable<BagCell>>? orderBy = null,
        Func<IQueryable<BagCell>, IIncludableQueryable<BagCell, object>>? include = null,
        int index = 0,
        int size = 10,
        bool withDeleted = false,
        bool enableTracking = true,
        CancellationToken cancellationToken = default
    ) =>
        await _bagCellRepository.GetListAsync(
            predicate,
            orderBy,
            include,
            index,
            size,
            withDeleted,
            enableTracking,
            cancellationToken
        );

    public async Task<BagCell> AddAsync(BagCell bagCell) => await _bagCellRepository.AddAsync(bagCell);

    public async Task<ICollection<BagCell>> AddRangeAsync(ICollection<BagCell> bagCells) =>
        await _bagCellRepository.AddRangeAsync(bagCells);

    public async Task<BagCell> UpdateAsync(BagCell bagCell) => await _bagCellRepository.UpdateAsync(bagCell);

    public async Task<ICollection<BagCell>> UpdateRangeAsync(ICollection<BagCell> bagCells) =>
        await _bagCellRepository.UpdateRangeAsync(bagCells);

    public async Task<BagCell> DeleteAsync(BagCell bagCell, bool permanent = true) =>
        await _bagCellRepository.DeleteAsync(bagCell, permanent);

    public async Task<ICollection<BagCell>> DeleteRangeAsync(ICollection<BagCell> bagCells, bool permanent = true) =>
        await _bagCellRepository.DeleteRangeAsync(bagCells, permanent);
}
