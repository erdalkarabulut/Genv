using Application.Features.Tanks.Constants;
using Application.Services.Repositories;
using NArchitecture.Core.Application.Rules;
using NArchitecture.Core.CrossCuttingConcerns.Exception.Types;
using NArchitecture.Core.Localization.Abstraction;
using Domain.Entities;

namespace Application.Features.Tanks.Rules;

public class TankBusinessRules : BaseBusinessRules
{
    private readonly ITankRepository _tankRepository;
    private readonly ILocalizationService _localizationService;

    public TankBusinessRules(ITankRepository tankRepository, ILocalizationService localizationService)
    {
        _tankRepository = tankRepository;
        _localizationService = localizationService;
    }

    private async Task throwBusinessException(string messageKey)
    {
        string message = await _localizationService.GetLocalizedAsync(messageKey, TanksBusinessMessages.SectionName);
        throw new BusinessException(message);
    }

    public async Task TankShouldExistWhenSelected(Tank? tank)
    {
        if (tank == null)
            await throwBusinessException(TanksBusinessMessages.TankNotExists);
    }

    public async Task TankIdShouldExistWhenSelected(Guid id, CancellationToken cancellationToken)
    {
        Tank? tank = await _tankRepository.GetAsync(
            predicate: t => t.Id == id,
            enableTracking: false,
            cancellationToken: cancellationToken
        );
        await TankShouldExistWhenSelected(tank);
    }
}