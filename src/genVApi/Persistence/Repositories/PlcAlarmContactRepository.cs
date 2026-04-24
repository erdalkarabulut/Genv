using Application.Services.Repositories;
using Domain.Entities;
using NArchitecture.Core.Persistence.Repositories;
using Persistence.Contexts;

namespace Persistence.Repositories;

public class PlcAlarmContactRepository : EfRepositoryBase<PlcAlarmContact, Guid, BaseDbContext>, IPlcAlarmContactRepository
{
    public PlcAlarmContactRepository(BaseDbContext context)
        : base(context)
    {
    }
}
