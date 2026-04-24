using Domain.ValueObjects;

namespace Application.Services.ClinicalConfiguration;

public interface IClinicalThresholdsAccessor
{
    Task<ClinicalThresholds> GetAsync(CancellationToken cancellationToken = default);
}
