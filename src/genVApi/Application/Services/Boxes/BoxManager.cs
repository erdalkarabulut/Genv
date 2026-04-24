using System.Collections.Generic;
using Application.Features.Boxes.Rules;
using Application.Services.Repositories;
using NArchitecture.Core.Persistence.Paging;
using Domain.Entities;
using Microsoft.EntityFrameworkCore.Query;
using System.Linq.Expressions;

namespace Application.Services.Boxes;

public class BoxManager : IBoxService
{
    private readonly IBoxRepository _boxRepository;
    private readonly BoxBusinessRules _boxBusinessRules;

    public BoxManager(IBoxRepository boxRepository, BoxBusinessRules boxBusinessRules)
    {
        _boxRepository = boxRepository;
        _boxBusinessRules = boxBusinessRules;
    }

    public async Task<Box?> GetAsync(
        Expression<Func<Box, bool>> predicate,
        Func<IQueryable<Box>, IIncludableQueryable<Box, object>>? include = null,
        bool withDeleted = false,
        bool enableTracking = true,
        CancellationToken cancellationToken = default
    )
    {
        Box? box = await _boxRepository.GetAsync(predicate, include, withDeleted, enableTracking, cancellationToken);
        return box;
    }

    public async Task<IPaginate<Box>?> GetListAsync(
        Expression<Func<Box, bool>>? predicate = null,
        Func<IQueryable<Box>, IOrderedQueryable<Box>>? orderBy = null,
        Func<IQueryable<Box>, IIncludableQueryable<Box, object>>? include = null,
        int index = 0,
        int size = 10,
        bool withDeleted = false,
        bool enableTracking = true,
        CancellationToken cancellationToken = default
    )
    {
        IPaginate<Box> boxList = await _boxRepository.GetListAsync(
            predicate,
            orderBy,
            include,
            index,
            size,
            withDeleted,
            enableTracking,
            cancellationToken
        );
        return boxList;
    }

    public async Task<Box> AddAsync(Box box)
    {
        Box addedBox = await _boxRepository.AddAsync(box);

        return addedBox;
    }

    public async Task<ICollection<Box>> AddRangeAsync(ICollection<Box> boxs)
    {
        ICollection<Box> addedBoxs = await _boxRepository.AddRangeAsync(boxs);

        return addedBoxs;
    }

    public async Task<Box> UpdateAsync(Box box)
    {
        Box updatedBox = await _boxRepository.UpdateAsync(box);

        return updatedBox;
    }

    public async Task<ICollection<Box>> UpdateRangeAsync(ICollection<Box> boxs)
    {
        ICollection<Box> updatedBoxs = await _boxRepository.UpdateRangeAsync(boxs);

        return updatedBoxs;
    }

    public async Task<Box> DeleteAsync(Box box, bool permanent = false)
    {
        Box deletedBox = await _boxRepository.DeleteAsync(box);

        return deletedBox;
    }

    public async Task<ICollection<Box>> DeleteRangeAsync(ICollection<Box> boxs, bool permanent = false)
    {
        ICollection<Box> deletedBoxs = await _boxRepository.DeleteRangeAsync(boxs, permanent);

        return deletedBoxs;
    }
}
