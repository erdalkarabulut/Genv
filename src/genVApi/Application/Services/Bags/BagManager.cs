using System.Collections.Generic;
using Application.Features.Bags.Rules;
using Application.Services.Repositories;
using NArchitecture.Core.Persistence.Paging;
using Domain.Entities;
using Microsoft.EntityFrameworkCore.Query;
using System.Linq.Expressions;

namespace Application.Services.Bags;

public class BagManager : IBagService
{
    private readonly IBagRepository _bagRepository;
    private readonly BagBusinessRules _bagBusinessRules;

    public BagManager(IBagRepository bagRepository, BagBusinessRules bagBusinessRules)
    {
        _bagRepository = bagRepository;
        _bagBusinessRules = bagBusinessRules;
    }

    public async Task<Bag?> GetAsync(
        Expression<Func<Bag, bool>> predicate,
        Func<IQueryable<Bag>, IIncludableQueryable<Bag, object>>? include = null,
        bool withDeleted = false,
        bool enableTracking = true,
        CancellationToken cancellationToken = default
    )
    {
        Bag? bag = await _bagRepository.GetAsync(predicate, include, withDeleted, enableTracking, cancellationToken);
        return bag;
    }

    public async Task<IPaginate<Bag>?> GetListAsync(
        Expression<Func<Bag, bool>>? predicate = null,
        Func<IQueryable<Bag>, IOrderedQueryable<Bag>>? orderBy = null,
        Func<IQueryable<Bag>, IIncludableQueryable<Bag, object>>? include = null,
        int index = 0,
        int size = 10,
        bool withDeleted = false,
        bool enableTracking = true,
        CancellationToken cancellationToken = default
    )
    {
        IPaginate<Bag> bagList = await _bagRepository.GetListAsync(
            predicate,
            orderBy,
            include,
            index,
            size,
            withDeleted,
            enableTracking,
            cancellationToken
        );
        return bagList;
    }

    public async Task<Bag> AddAsync(Bag bag)
    {
        Bag addedBag = await _bagRepository.AddAsync(bag);

        return addedBag;
    }

    public async Task<ICollection<Bag>> AddRangeAsync(ICollection<Bag> bags)
    {
        ICollection<Bag> addedBags = await _bagRepository.AddRangeAsync(bags);

        return addedBags;
    }

    public async Task<Bag> UpdateAsync(Bag bag)
    {
        Bag updatedBag = await _bagRepository.UpdateAsync(bag);

        return updatedBag;
    }

    public async Task<ICollection<Bag>> UpdateRangeAsync(ICollection<Bag> bags)
    {
        ICollection<Bag> updatedBags = await _bagRepository.UpdateRangeAsync(bags);

        return updatedBags;
    }

    public async Task<Bag> DeleteAsync(Bag bag, bool permanent = true)
    {
        Bag deletedBag = await _bagRepository.DeleteAsync(bag, permanent);

        return deletedBag;
    }

    public async Task<ICollection<Bag>> DeleteRangeAsync(ICollection<Bag> bags, bool permanent = true)
    {
        ICollection<Bag> deletedBags = await _bagRepository.DeleteRangeAsync(bags, permanent);

        return deletedBags;
    }
}
