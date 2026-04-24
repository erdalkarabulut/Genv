using Domain.Entities;
using NArchitecture.Core.Persistence.Repositories;

namespace Application.Services.Repositories;

public interface IPlcSensorPointRepository : IAsyncRepository<PlcSensorPoint, Guid>, IRepository<PlcSensorPoint, Guid>
{
}
