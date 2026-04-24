using Application.Services.Repositories;
using ClinicalEntity = Domain.Entities.ClinicalSettings;
using Domain.ValueObjects;

namespace Application.Services.ClinicalConfiguration;

public class ClinicalThresholdsAccessor : IClinicalThresholdsAccessor
{
    private readonly IClinicalSettingsRepository _repository;

    public ClinicalThresholdsAccessor(IClinicalSettingsRepository repository) => _repository = repository;

    public async Task<ClinicalThresholds> GetAsync(CancellationToken cancellationToken = default)
    {
        ClinicalEntity? row = await _repository.GetAsync(
            predicate: x => x.Id == ClinicalEntity.SingletonId,
            cancellationToken: cancellationToken);
        return row is null ? ClinicalThresholds.Default : ClinicalThresholds.FromEntity(row);
    }
}
