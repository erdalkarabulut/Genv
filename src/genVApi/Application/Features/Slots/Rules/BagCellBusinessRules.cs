using Application.Features.Slots.Constants;
using Application.Services.Repositories;
using NArchitecture.Core.Application.Rules;
using NArchitecture.Core.CrossCuttingConcerns.Exception.Types;
using NArchitecture.Core.Localization.Abstraction;
using Domain.Entities;

namespace Application.Features.Slots.Rules;

public class BagCellBusinessRules : BaseBusinessRules
{
    private readonly IBagCellRepository _bagCellRepository;
    private readonly ILocalizationService _localizationService;

    public BagCellBusinessRules(IBagCellRepository bagCellRepository, ILocalizationService localizationService)
    {
        _bagCellRepository = bagCellRepository;
        _localizationService = localizationService;
    }

    private async Task throwBusinessException(string messageKey)
    {
        string message = await _localizationService.GetLocalizedAsync(messageKey, SlotsBusinessMessages.SectionName);
        throw new BusinessException(message);
    }

    public async Task BagCellShouldExistWhenSelected(BagCell? bagCell)
    {
        if (bagCell == null)
            await throwBusinessException(SlotsBusinessMessages.SlotNotExists);
    }

    public async Task BagCellIdShouldExistWhenSelected(Guid id, CancellationToken cancellationToken)
    {
        BagCell? bagCell = await _bagCellRepository.GetAsync(
            predicate: c => c.Id == id,
            enableTracking: false,
            cancellationToken: cancellationToken
        );
        await BagCellShouldExistWhenSelected(bagCell);
    }
}