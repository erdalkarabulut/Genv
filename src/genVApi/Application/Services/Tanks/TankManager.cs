using System.Collections.Generic;
using Application.Features.Tanks.Rules;
using Application.Services.Repositories;
using NArchitecture.Core.Persistence.Paging;
using Domain.Entities;
using Microsoft.EntityFrameworkCore.Query;
using System.Linq.Expressions;

namespace Application.Services.Tanks;

public class TankManager : ITankService
{
    private readonly ITankRepository _tankRepository;
    private readonly TankBusinessRules _tankBusinessRules;

    public TankManager(ITankRepository tankRepository, TankBusinessRules tankBusinessRules)
    {
        _tankRepository = tankRepository;
        _tankBusinessRules = tankBusinessRules;
    }

    public async Task<Tank?> GetAsync(
        Expression<Func<Tank, bool>> predicate,
        Func<IQueryable<Tank>, IIncludableQueryable<Tank, object>>? include = null,
        bool withDeleted = false,
        bool enableTracking = true,
        CancellationToken cancellationToken = default
    )
    {
        Tank? tank = await _tankRepository.GetAsync(predicate, include, withDeleted, enableTracking, cancellationToken);
        return tank;
    }

    public async Task<IPaginate<Tank>?> GetListAsync(
        Expression<Func<Tank, bool>>? predicate = null,
        Func<IQueryable<Tank>, IOrderedQueryable<Tank>>? orderBy = null,
        Func<IQueryable<Tank>, IIncludableQueryable<Tank, object>>? include = null,
        int index = 0,
        int size = 10,
        bool withDeleted = false,
        bool enableTracking = true,
        CancellationToken cancellationToken = default
    )
    {
        IPaginate<Tank> tankList = await _tankRepository.GetListAsync(
            predicate,
            orderBy,
            include,
            index,
            size,
            withDeleted,
            enableTracking,
            cancellationToken
        );
        return tankList;
    }

    public async Task<Tank> AddAsync(Tank tank)
    {
        Tank addedTank = await _tankRepository.AddAsync(tank);

        return addedTank;
    }

    public async Task<ICollection<Tank>> AddRangeAsync(ICollection<Tank> tanks)
    {
        ICollection<Tank> addedTanks = await _tankRepository.AddRangeAsync(tanks);

        return addedTanks;
    }

    public async Task<Tank> UpdateAsync(Tank tank)
    {
        Tank updatedTank = await _tankRepository.UpdateAsync(tank);

        return updatedTank;
    }

    public async Task<ICollection<Tank>> UpdateRangeAsync(ICollection<Tank> tanks)
    {
        ICollection<Tank> updatedTanks = await _tankRepository.UpdateRangeAsync(tanks);

        return updatedTanks;
    }

    public async Task<Tank> DeleteAsync(Tank tank, bool permanent = false)
    {
        Tank deletedTank = await _tankRepository.DeleteAsync(tank);

        return deletedTank;
    }

    public async Task<ICollection<Tank>> DeleteRangeAsync(ICollection<Tank> tanks, bool permanent = false)
    {
        ICollection<Tank> deletedTanks = await _tankRepository.DeleteRangeAsync(tanks, permanent);

        return deletedTanks;
    }
}
