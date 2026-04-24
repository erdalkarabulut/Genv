using Application.Features.Boxes.Constants;
using Application.Services.Repositories;
using NArchitecture.Core.Application.Rules;
using NArchitecture.Core.CrossCuttingConcerns.Exception.Types;
using NArchitecture.Core.Localization.Abstraction;
using Domain.Entities;

namespace Application.Features.Boxes.Rules;

public class BoxBusinessRules : BaseBusinessRules
{
    private readonly IBoxRepository _boxRepository;
    private readonly ILocalizationService _localizationService;

    public BoxBusinessRules(IBoxRepository boxRepository, ILocalizationService localizationService)
    {
        _boxRepository = boxRepository;
        _localizationService = localizationService;
    }

    private async Task throwBusinessException(string messageKey)
    {
        string message = await _localizationService.GetLocalizedAsync(messageKey, BoxesBusinessMessages.SectionName);
        throw new BusinessException(message);
    }

    public async Task BoxShouldExistWhenSelected(Box? box)
    {
        if (box == null)
            await throwBusinessException(BoxesBusinessMessages.BoxNotExists);
    }

    public async Task BoxIdShouldExistWhenSelected(Guid id, CancellationToken cancellationToken)
    {
        Box? box = await _boxRepository.GetAsync(
            predicate: b => b.Id == id,
            enableTracking: false,
            cancellationToken: cancellationToken
        );
        await BoxShouldExistWhenSelected(box);
    }
}