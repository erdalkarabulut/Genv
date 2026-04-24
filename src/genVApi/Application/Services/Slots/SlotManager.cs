using System.Collections.Generic;
using Application.Features.Slots.Rules;
using Application.Services.Repositories;
using NArchitecture.Core.Persistence.Paging;
using Domain.Entities;
using Microsoft.EntityFrameworkCore.Query;
using System.Linq.Expressions;

namespace Application.Services.Slots;

public class SlotManager : ISlotService
{
    private readonly ISlotRepository _slotRepository;
    private readonly SlotBusinessRules _slotBusinessRules;

    public SlotManager(ISlotRepository slotRepository, SlotBusinessRules slotBusinessRules)
    {
        _slotRepository = slotRepository;
        _slotBusinessRules = slotBusinessRules;
    }

    public async Task<Slot?> GetAsync(
        Expression<Func<Slot, bool>> predicate,
        Func<IQueryable<Slot>, IIncludableQueryable<Slot, object>>? include = null,
        bool withDeleted = false,
        bool enableTracking = true,
        CancellationToken cancellationToken = default
    )
    {
        Slot? slot = await _slotRepository.GetAsync(predicate, include, withDeleted, enableTracking, cancellationToken);
        return slot;
    }

    public async Task<IPaginate<Slot>?> GetListAsync(
        Expression<Func<Slot, bool>>? predicate = null,
        Func<IQueryable<Slot>, IOrderedQueryable<Slot>>? orderBy = null,
        Func<IQueryable<Slot>, IIncludableQueryable<Slot, object>>? include = null,
        int index = 0,
        int size = 10,
        bool withDeleted = false,
        bool enableTracking = true,
        CancellationToken cancellationToken = default
    )
    {
        IPaginate<Slot> slotList = await _slotRepository.GetListAsync(
            predicate,
            orderBy,
            include,
            index,
            size,
            withDeleted,
            enableTracking,
            cancellationToken
        );
        return slotList;
    }

    public async Task<Slot> AddAsync(Slot slot)
    {
        Slot addedSlot = await _slotRepository.AddAsync(slot);

        return addedSlot;
    }

    public async Task<ICollection<Slot>> AddRangeAsync(ICollection<Slot> slots)
    {
        ICollection<Slot> addedSlots = await _slotRepository.AddRangeAsync(slots);

        return addedSlots;
    }

    public async Task<Slot> UpdateAsync(Slot slot)
    {
        Slot updatedSlot = await _slotRepository.UpdateAsync(slot);

        return updatedSlot;
    }

    public async Task<ICollection<Slot>> UpdateRangeAsync(ICollection<Slot> slots)
    {
        ICollection<Slot> updatedSlots = await _slotRepository.UpdateRangeAsync(slots);

        return updatedSlots;
    }

    public async Task<Slot> DeleteAsync(Slot slot, bool permanent = false)
    {
        Slot deletedSlot = await _slotRepository.DeleteAsync(slot);

        return deletedSlot;
    }

    public async Task<ICollection<Slot>> DeleteRangeAsync(ICollection<Slot> slots, bool permanent = false)
    {
        ICollection<Slot> deletedSlots = await _slotRepository.DeleteRangeAsync(slots, permanent);

        return deletedSlots;
    }
}
