using Application.Services.Repositories;
using Domain.Entities;
using NArchitecture.Core.Persistence.Repositories;
using Persistence.Contexts;

namespace Persistence.Repositories;

public class PlcSensorPointRepository : EfRepositoryBase<PlcSensorPoint, Guid, BaseDbContext>, IPlcSensorPointRepository
{
    public PlcSensorPointRepository(BaseDbContext context)
        : base(context)
    {
    }
}
