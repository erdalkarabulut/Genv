using Application.Services.Repositories;
using Domain.Entities;
using NArchitecture.Core.Persistence.Repositories;
using Persistence.Contexts;

namespace Persistence.Repositories;

public class PlcAlarmTemplateRepository : EfRepositoryBase<PlcAlarmTemplate, Guid, BaseDbContext>, IPlcAlarmTemplateRepository
{
    public PlcAlarmTemplateRepository(BaseDbContext context)
        : base(context)
    {
    }
}
