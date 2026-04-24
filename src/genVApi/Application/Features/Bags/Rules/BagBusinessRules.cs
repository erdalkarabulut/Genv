using Application.Features.Bags.Constants;
using Application.Services.Repositories;
using NArchitecture.Core.Application.Rules;
using NArchitecture.Core.CrossCuttingConcerns.Exception.Types;
using NArchitecture.Core.Localization.Abstraction;
using Domain.Entities;

namespace Application.Features.Bags.Rules;

public class BagBusinessRules : BaseBusinessRules
{
    private readonly IBagRepository _bagRepository;
    private readonly ILocalizationService _localizationService;

    public BagBusinessRules(IBagRepository bagRepository, ILocalizationService localizationService)
    {
        _bagRepository = bagRepository;
        _localizationService = localizationService;
    }

    private async Task throwBusinessException(string messageKey)
    {
        string message = await _localizationService.GetLocalizedAsync(messageKey, BagsBusinessMessages.SectionName);
        throw new BusinessException(message);
    }

    public async Task BagShouldExistWhenSelected(Bag? bag)
    {
        if (bag == null)
            await throwBusinessException(BagsBusinessMessages.BagNotExists);
    }

    public async Task BagIdShouldExistWhenSelected(Guid id, CancellationToken cancellationToken)
    {
        Bag? bag = await _bagRepository.GetAsync(
            predicate: b => b.Id == id,
            enableTracking: false,
            cancellationToken: cancellationToken
        );
        await BagShouldExistWhenSelected(bag);
    }
}