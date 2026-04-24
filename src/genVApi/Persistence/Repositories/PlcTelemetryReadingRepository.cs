using Application.Services.Repositories;
using Domain.Entities;
using NArchitecture.Core.Persistence.Repositories;
using Persistence.Contexts;

namespace Persistence.Repositories;

public class PlcTelemetryReadingRepository : EfRepositoryBase<PlcTelemetryReading, Guid, BaseDbContext>, IPlcTelemetryReadingRepository
{
    public PlcTelemetryReadingRepository(BaseDbContext context)
        : base(context)
    {
    }
}
