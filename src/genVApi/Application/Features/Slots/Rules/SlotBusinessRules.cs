using Application.Features.Slots.Constants;
using Application.Services.Repositories;
using NArchitecture.Core.Application.Rules;
using NArchitecture.Core.CrossCuttingConcerns.Exception.Types;
using NArchitecture.Core.Localization.Abstraction;
using Domain.Entities;

namespace Application.Features.Slots.Rules;

public class SlotBusinessRules : BaseBusinessRules
{
    private readonly ISlotRepository _slotRepository;
    private readonly ILocalizationService _localizationService;

    public SlotBusinessRules(ISlotRepository slotRepository, ILocalizationService localizationService)
    {
        _slotRepository = slotRepository;
        _localizationService = localizationService;
    }

    private async Task throwBusinessException(string messageKey)
    {
        string message = await _localizationService.GetLocalizedAsync(messageKey, SlotsBusinessMessages.SectionName);
        throw new BusinessException(message);
    }

    public async Task SlotShouldExistWhenSelected(Slot? slot)
    {
        if (slot == null)
            await throwBusinessException(SlotsBusinessMessages.SlotNotExists);
    }

    public async Task SlotIdShouldExistWhenSelected(Guid id, CancellationToken cancellationToken)
    {
        Slot? slot = await _slotRepository.GetAsync(
            predicate: s => s.Id == id,
            enableTracking: false,
            cancellationToken: cancellationToken
        );
        await SlotShouldExistWhenSelected(slot);
    }
}