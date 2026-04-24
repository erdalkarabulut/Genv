using NArchitecture.Core.Persistence.Repositories;

namespace Application.Services.Repositories;

public interface IClinicalSettingsRepository
    : IAsyncRepository<Domain.Entities.ClinicalSettings, Guid>,
        IRepository<Domain.Entities.ClinicalSettings, Guid>;
