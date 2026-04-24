using System.Collections.Generic;
using Application.Features.Racks.Rules;
using Application.Services.Repositories;
using NArchitecture.Core.Persistence.Paging;
using Domain.Entities;
using Microsoft.EntityFrameworkCore.Query;
using System.Linq.Expressions;

namespace Application.Services.Racks;

public class RackManager : IRackService
{
    private readonly IRackRepository _rackRepository;
    private readonly RackBusinessRules _rackBusinessRules;

    public RackManager(IRackRepository rackRepository, RackBusinessRules rackBusinessRules)
    {
        _rackRepository = rackRepository;
        _rackBusinessRules = rackBusinessRules;
    }

    public async Task<Rack?> GetAsync(
        Expression<Func<Rack, bool>> predicate,
        Func<IQueryable<Rack>, IIncludableQueryable<Rack, object>>? include = null,
        bool withDeleted = false,
        bool enableTracking = true,
        CancellationToken cancellationToken = default
    )
    {
        Rack? rack = await _rackRepository.GetAsync(predicate, include, withDeleted, enableTracking, cancellationToken);
        return rack;
    }

    public async Task<IPaginate<Rack>?> GetListAsync(
        Expression<Func<Rack, bool>>? predicate = null,
        Func<IQueryable<Rack>, IOrderedQueryable<Rack>>? orderBy = null,
        Func<IQueryable<Rack>, IIncludableQueryable<Rack, object>>? include = null,
        int index = 0,
        int size = 10,
        bool withDeleted = false,
        bool enableTracking = true,
        CancellationToken cancellationToken = default
    )
    {
        IPaginate<Rack> rackList = await _rackRepository.GetListAsync(
            predicate,
            orderBy,
            include,
            index,
            size,
            withDeleted,
            enableTracking,
            cancellationToken
        );
        return rackList;
    }

    public async Task<Rack> AddAsync(Rack rack)
    {
        Rack addedRack = await _rackRepository.AddAsync(rack);

        return addedRack;
    }

    public async Task<ICollection<Rack>> AddRangeAsync(ICollection<Rack> racks)
    {
        ICollection<Rack> addedRacks = await _rackRepository.AddRangeAsync(racks);

        return addedRacks;
    }

    public async Task<Rack> UpdateAsync(Rack rack)
    {
        Rack updatedRack = await _rackRepository.UpdateAsync(rack);

        return updatedRack;
    }

    public async Task<ICollection<Rack>> UpdateRangeAsync(ICollection<Rack> racks)
    {
        ICollection<Rack> updatedRacks = await _rackRepository.UpdateRangeAsync(racks);

        return updatedRacks;
    }

    public async Task<Rack> DeleteAsync(Rack rack, bool permanent = false)
    {
        Rack deletedRack = await _rackRepository.DeleteAsync(rack);

        return deletedRack;
    }

    public async Task<ICollection<Rack>> DeleteRangeAsync(ICollection<Rack> racks, bool permanent = false)
    {
        ICollection<Rack> deletedRacks = await _rackRepository.DeleteRangeAsync(racks, permanent);

        return deletedRacks;
    }
}
