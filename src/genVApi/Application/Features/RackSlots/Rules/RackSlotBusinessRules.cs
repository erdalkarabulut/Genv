using Application.Services.Repositories;
using NArchitecture.Core.Application.Rules;
using NArchitecture.Core.CrossCuttingConcerns.Exception.Types;
using Domain.Entities;

namespace Application.Features.RackSlots.Rules;

public class RackSlotBusinessRules : BaseBusinessRules
{
    private readonly ISlotRepository _rackSlotRepository;

    public RackSlotBusinessRules(ISlotRepository rackSlotRepository)
    {
        _rackSlotRepository = rackSlotRepository;
    }

    public Task RackSlotShouldExistWhenSelected(Slot? slot)
    {
        if (slot is null)
            throw new BusinessException("Raf slotu bulunamadı.");
        return Task.CompletedTask;
    }

    public async Task RackSlotIdShouldExistWhenSelected(Guid id, CancellationToken cancellationToken)
    {
        Slot? slot = await _rackSlotRepository.GetAsync(
            predicate: s => s.Id == id,
            enableTracking: false,
            cancellationToken: cancellationToken
        );
        await RackSlotShouldExistWhenSelected(slot);
    }
}
