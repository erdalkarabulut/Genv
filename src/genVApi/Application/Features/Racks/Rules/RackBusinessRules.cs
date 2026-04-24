using Application.Features.Racks.Constants;
using Application.Services.Repositories;
using NArchitecture.Core.Application.Rules;
using NArchitecture.Core.CrossCuttingConcerns.Exception.Types;
using NArchitecture.Core.Localization.Abstraction;
using Domain.Entities;

namespace Application.Features.Racks.Rules;

public class RackBusinessRules : BaseBusinessRules
{
    private readonly IRackRepository _rackRepository;
    private readonly ILocalizationService _localizationService;

    public RackBusinessRules(IRackRepository rackRepository, ILocalizationService localizationService)
    {
        _rackRepository = rackRepository;
        _localizationService = localizationService;
    }

    private async Task throwBusinessException(string messageKey)
    {
        string message = await _localizationService.GetLocalizedAsync(messageKey, RacksBusinessMessages.SectionName);
        throw new BusinessException(message);
    }

    public async Task RackShouldExistWhenSelected(Rack? rack)
    {
        if (rack == null)
            await throwBusinessException(RacksBusinessMessages.RackNotExists);
    }

    public async Task RackIdShouldExistWhenSelected(Guid id, CancellationToken cancellationToken)
    {
        Rack? rack = await _rackRepository.GetAsync(
            predicate: r => r.Id == id,
            enableTracking: false,
            cancellationToken: cancellationToken
        );
        await RackShouldExistWhenSelected(rack);
    }
}