using Application.Features.BagMovements.Constants;
using Application.Services.Repositories;
using NArchitecture.Core.Application.Rules;
using NArchitecture.Core.CrossCuttingConcerns.Exception.Types;
using NArchitecture.Core.Localization.Abstraction;
using Domain.Entities;

namespace Application.Features.BagMovements.Rules;

public class BagMovementBusinessRules : BaseBusinessRules
{
    private readonly IBagMovementRepository _bagMovementRepository;
    private readonly ILocalizationService _localizationService;

    public BagMovementBusinessRules(IBagMovementRepository bagMovementRepository, ILocalizationService localizationService)
    {
        _bagMovementRepository = bagMovementRepository;
        _localizationService = localizationService;
    }

    private async Task throwBusinessException(string messageKey)
    {
        string message = await _localizationService.GetLocalizedAsync(messageKey, BagMovementsBusinessMessages.SectionName);
        throw new BusinessException(message);
    }

    public async Task BagMovementShouldExistWhenSelected(BagMovement? bagMovement)
    {
        if (bagMovement == null)
            await throwBusinessException(BagMovementsBusinessMessages.BagMovementNotExists);
    }

    public async Task BagMovementIdShouldExistWhenSelected(Guid id, CancellationToken cancellationToken)
    {
        BagMovement? bagMovement = await _bagMovementRepository.GetAsync(
            predicate: bm => bm.Id == id,
            enableTracking: false,
            cancellationToken: cancellationToken
        );
        await BagMovementShouldExistWhenSelected(bagMovement);
    }
}