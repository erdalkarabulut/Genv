using Application.Features.Donors.Constants;
using Application.Services.Repositories;
using NArchitecture.Core.Application.Rules;
using NArchitecture.Core.CrossCuttingConcerns.Exception.Types;
using NArchitecture.Core.Localization.Abstraction;
using Domain.Entities;

namespace Application.Features.Donors.Rules;

public class DonorBusinessRules : BaseBusinessRules
{
    private readonly IDonorRepository _donorRepository;
    private readonly ILocalizationService _localizationService;

    public DonorBusinessRules(IDonorRepository donorRepository, ILocalizationService localizationService)
    {
        _donorRepository = donorRepository;
        _localizationService = localizationService;
    }

    private async Task throwBusinessException(string messageKey)
    {
        string message = await _localizationService.GetLocalizedAsync(messageKey, DonorsBusinessMessages.SectionName);
        throw new BusinessException(message);
    }

    public async Task DonorShouldExistWhenSelected(Donor? donor)
    {
        if (donor == null)
            await throwBusinessException(DonorsBusinessMessages.DonorNotExists);
    }

    public async Task DonorIdShouldExistWhenSelected(Guid id, CancellationToken cancellationToken)
    {
        Donor? donor = await _donorRepository.GetAsync(
            predicate: d => d.Id == id,
            enableTracking: false,
            cancellationToken: cancellationToken
        );
        await DonorShouldExistWhenSelected(donor);
    }
}