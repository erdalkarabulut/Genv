using System.Collections.Generic;
using Application.Features.BagMovements.Rules;
using Application.Services.Repositories;
using NArchitecture.Core.Persistence.Paging;
using Domain.Entities;
using Microsoft.EntityFrameworkCore.Query;
using System.Linq.Expressions;

namespace Application.Services.BagMovements;

public class BagMovementManager : IBagMovementService
{
    private readonly IBagMovementRepository _bagMovementRepository;
    private readonly BagMovementBusinessRules _bagMovementBusinessRules;

    public BagMovementManager(IBagMovementRepository bagMovementRepository, BagMovementBusinessRules bagMovementBusinessRules)
    {
        _bagMovementRepository = bagMovementRepository;
        _bagMovementBusinessRules = bagMovementBusinessRules;
    }

    public async Task<BagMovement?> GetAsync(
        Expression<Func<BagMovement, bool>> predicate,
        Func<IQueryable<BagMovement>, IIncludableQueryable<BagMovement, object>>? include = null,
        bool withDeleted = false,
        bool enableTracking = true,
        CancellationToken cancellationToken = default
    )
    {
        BagMovement? bagMovement = await _bagMovementRepository.GetAsync(predicate, include, withDeleted, enableTracking, cancellationToken);
        return bagMovement;
    }

    public async Task<IPaginate<BagMovement>?> GetListAsync(
        Expression<Func<BagMovement, bool>>? predicate = null,
        Func<IQueryable<BagMovement>, IOrderedQueryable<BagMovement>>? orderBy = null,
        Func<IQueryable<BagMovement>, IIncludableQueryable<BagMovement, object>>? include = null,
        int index = 0,
        int size = 10,
        bool withDeleted = false,
        bool enableTracking = true,
        CancellationToken cancellationToken = default
    )
    {
        IPaginate<BagMovement> bagMovementList = await _bagMovementRepository.GetListAsync(
            predicate,
            orderBy,
            include,
            index,
            size,
            withDeleted,
            enableTracking,
            cancellationToken
        );
        return bagMovementList;
    }

    public async Task<BagMovement> AddAsync(BagMovement bagMovement)
    {
        BagMovement addedBagMovement = await _bagMovementRepository.AddAsync(bagMovement);

        return addedBagMovement;
    }

    public async Task<ICollection<BagMovement>> AddRangeAsync(ICollection<BagMovement> bagMovements)
    {
        ICollection<BagMovement> addedBagMovements = await _bagMovementRepository.AddRangeAsync(bagMovements);

        return addedBagMovements;
    }

    public async Task<BagMovement> UpdateAsync(BagMovement bagMovement)
    {
        BagMovement updatedBagMovement = await _bagMovementRepository.UpdateAsync(bagMovement);

        return updatedBagMovement;
    }

    public async Task<ICollection<BagMovement>> UpdateRangeAsync(ICollection<BagMovement> bagMovements)
    {
        ICollection<BagMovement> updatedBagMovements = await _bagMovementRepository.UpdateRangeAsync(bagMovements);

        return updatedBagMovements;
    }

    public async Task<BagMovement> DeleteAsync(BagMovement bagMovement, bool permanent = true)
    {
        BagMovement deletedBagMovement = await _bagMovementRepository.DeleteAsync(bagMovement, permanent);

        return deletedBagMovement;
    }

    public async Task<ICollection<BagMovement>> DeleteRangeAsync(ICollection<BagMovement> bagMovements, bool permanent = true)
    {
        ICollection<BagMovement> deletedBagMovements = await _bagMovementRepository.DeleteRangeAsync(bagMovements, permanent);

        return deletedBagMovements;
    }
}
