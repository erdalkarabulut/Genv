using Application.Features.CollectionSessions.Constants;
using Application.Services.ClinicalConfiguration;
using Application.Services.Repositories;
using Microsoft.EntityFrameworkCore;
using NArchitecture.Core.Application.Rules;
using NArchitecture.Core.CrossCuttingConcerns.Exception.Types;
using NArchitecture.Core.Localization.Abstraction;
using Domain.Entities;
using Domain.ValueObjects;

namespace Application.Features.CollectionSessions.Rules;

public class CollectionSessionBusinessRules : BaseBusinessRules
{
    private readonly ICollectionSessionRepository _collectionSessionRepository;
    private readonly IPatientRepository _patientRepository;
    private readonly IClinicalThresholdsAccessor _clinicalThresholdsAccessor;
    private readonly ILocalizationService _localizationService;

    public CollectionSessionBusinessRules(
        ICollectionSessionRepository collectionSessionRepository,
        IPatientRepository patientRepository,
        IClinicalThresholdsAccessor clinicalThresholdsAccessor,
        ILocalizationService localizationService)
    {
        _collectionSessionRepository = collectionSessionRepository;
        _patientRepository = patientRepository;
        _clinicalThresholdsAccessor = clinicalThresholdsAccessor;
        _localizationService = localizationService;
    }

    private async Task throwBusinessException(string messageKey)
    {
        string message = await _localizationService.GetLocalizedAsync(messageKey, CollectionSessionsBusinessMessages.SectionName);
        throw new BusinessException(message);
    }

    public async Task CollectionSessionShouldExistWhenSelected(CollectionSession? collectionSession)
    {
        if (collectionSession == null)
            await throwBusinessException(CollectionSessionsBusinessMessages.CollectionSessionNotExists);
    }

    public async Task CollectionSessionIdShouldExistWhenSelected(Guid id, CancellationToken cancellationToken)
    {
        CollectionSession? collectionSession = await _collectionSessionRepository.GetAsync(
            predicate: cs => cs.Id == id,
            enableTracking: false,
            cancellationToken: cancellationToken
        );
        await CollectionSessionShouldExistWhenSelected(collectionSession);
    }

    /// <summary>
    /// Aferez gün sayısının transplant tipine göre izinli aralıkta olduğunu doğrular.
    /// Otolog: 4 gün, Allogeneik: 2 gün.
    /// </summary>
    public async Task SessionDayShouldBeInAllowedRange(Guid patientId, int day, Guid? excludeSessionId, CancellationToken cancellationToken)
    {
        if (day < 1)
            throw new BusinessException("Aferez günü 1'den küçük olamaz.");

        Patient? patient = await _patientRepository.GetAsync(
            predicate: p => p.Id == patientId,
            enableTracking: false,
            cancellationToken: cancellationToken
        );
        if (patient is null)
            throw new BusinessException("Hasta bulunamadı.");

        ClinicalThresholds thresholds = await _clinicalThresholdsAccessor.GetAsync(cancellationToken);
        int maxDays = patient.GetMaxCollectionDays(thresholds);
        if (day > maxDays)
        {
            var type = patient.IsAutologous() ? "otolog" : "allogeneik";
            throw new BusinessException($"Bu hasta {type} protokolünde maksimum {maxDays} gün aferez yapılabilir. Gün {day} izinli değildir.");
        }

        bool duplicate = await _collectionSessionRepository.Query()
            .AsNoTracking()
            .AnyAsync(s => s.PatientId == patientId && s.Day == day && (excludeSessionId == null || s.Id != excludeSessionId), cancellationToken);
        if (duplicate)
            throw new BusinessException($"Bu hasta için gün {day} aferez kaydı zaten mevcut.");
    }

    /// <summary>
    /// Allogeneik hastalarda günlerin ardışık olduğunu doğrular (gün 1 olmadan gün 2 açılamaz).
    /// </summary>
    public async Task DaysMustBeConsecutiveForAllogeneic(Guid patientId, int day, CancellationToken cancellationToken)
    {
        if (day <= 1) return;

        Patient? patient = await _patientRepository.GetAsync(
            predicate: p => p.Id == patientId,
            enableTracking: false,
            cancellationToken: cancellationToken
        );
        if (patient is null || patient.IsAutologous()) return;

        bool previousExists = await _collectionSessionRepository.Query()
            .AsNoTracking()
            .AnyAsync(s => s.PatientId == patientId && s.Day == day - 1, cancellationToken);
        if (!previousExists)
            throw new BusinessException($"Allogeneik protokolde gün {day} açılmadan önce gün {day - 1} kaydı olmalıdır (ardışık olmak zorunda).");
    }
}