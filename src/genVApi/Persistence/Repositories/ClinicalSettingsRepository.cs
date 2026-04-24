using Application.Services.Repositories;
using Domain.Entities;
using NArchitecture.Core.Persistence.Repositories;
using Persistence.Contexts;

namespace Persistence.Repositories;

public class ClinicalSettingsRepository : EfRepositoryBase<ClinicalSettings, Guid, BaseDbContext>, IClinicalSettingsRepository
{
    public ClinicalSettingsRepository(BaseDbContext context)
        : base(context)
    {
    }
}
