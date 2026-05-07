using Domain.Entities;
using NArchitecture.Core.Persistence.Repositories;

namespace Application.Services.Repositories;

public interface IPlcSystemAlarmRepository : IAsyncRepository<PlcSystemAlarm, Guid>, IRepository<PlcSystemAlarm, Guid>
{
    Task<bool> HasOpenAlarmAsync(string sensorCode, PlcSystemAlarmType type, CancellationToken ct = default);
    Task CheckAndCreateSensorTimeoutAlarmsAsync(int timeoutMinutes, CancellationToken ct = default);
    Task CreateModbusConnectionAlarmAsync(string devicePrefix, string? address, string message, CancellationToken ct = default);
}