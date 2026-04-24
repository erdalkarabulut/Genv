using Domain.Entities;
using NArchitecture.Core.Persistence.Repositories;

namespace Application.Services.Repositories;

public interface IPlcTelemetryReadingRepository : IAsyncRepository<PlcTelemetryReading, Guid>, IRepository<PlcTelemetryReading, Guid>
{
}
