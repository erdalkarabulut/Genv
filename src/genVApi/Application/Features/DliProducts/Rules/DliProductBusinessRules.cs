using Application.Features.DliProducts.Constants;
using Application.Services.Repositories;
using NArchitecture.Core.Application.Rules;
using NArchitecture.Core.CrossCuttingConcerns.Exception.Types;
using NArchitecture.Core.Localization.Abstraction;
using Domain.Entities;

namespace Application.Features.DliProducts.Rules;

public class DliProductBusinessRules : BaseBusinessRules
{
    private readonly IDliProductRepository _dliProductRepository;
    private readonly ILocalizationService _localizationService;

    public DliProductBusinessRules(IDliProductRepository dliProductRepository, ILocalizationService localizationService)
    {
        _dliProductRepository = dliProductRepository;
        _localizationService = localizationService;
    }

    private async Task throwBusinessException(string messageKey)
    {
        string message = await _localizationService.GetLocalizedAsync(messageKey, DliProductsBusinessMessages.SectionName);
        throw new BusinessException(message);
    }

    public async Task DliProductShouldExistWhenSelected(DliProduct? dliProduct)
    {
        if (dliProduct == null)
            await throwBusinessException(DliProductsBusinessMessages.DliProductNotExists);
    }

    public async Task DliProductIdShouldExistWhenSelected(Guid id, CancellationToken cancellationToken)
    {
        DliProduct? dliProduct = await _dliProductRepository.GetAsync(
            predicate: dp => dp.Id == id,
            enableTracking: false,
            cancellationToken: cancellationToken
        );
        await DliProductShouldExistWhenSelected(dliProduct);
    }
}