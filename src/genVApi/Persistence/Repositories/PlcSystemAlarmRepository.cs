using Application.Services.Repositories;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Persistence.Contexts;
using NArchitecture.Core.Persistence.Repositories;

namespace Persistence.Repositories;

public class PlcSystemAlarmRepository : EfRepositoryBase<PlcSystemAlarm, Guid, BaseDbContext>, IPlcSystemAlarmRepository
{
    public PlcSystemAlarmRepository(BaseDbContext context) : base(context) { }

    public async Task<bool> HasOpenAlarmAsync(string sensorCode, PlcSystemAlarmType type, CancellationToken ct = default)
    {
        return await Context.Set<PlcSystemAlarm>().AnyAsync(
            a => a.SensorCode == sensorCode && a.Type == type && !a.IsResolved,
            ct);
    }

    public async Task CheckAndCreateSensorTimeoutAlarmsAsync(int timeoutMinutes, CancellationToken ct = default)
    {
        var threshold = DateTime.UtcNow.AddMinutes(-timeoutMinutes);

        // Timeout olan sensör noktalarını bul — en son okuma süresi threshold'dan önce olanlar
        var timedOutSensors = await Context.Set<PlcTelemetryReading>()
            .Where(r => r.ReadAtUtc < threshold)
            .Select(r => r.SensorPointId)
            .Distinct()
            .ToListAsync(ct);

        var sensorPoints = await Context.Set<PlcSensorPoint>()
            .Where(p => timedOutSensors.Contains(p.Id))
            .ToListAsync(ct);

        var alarmSet = Context.Set<PlcSystemAlarm>();

        foreach (var point in sensorPoints)
        {
            // Açık timeout alarmı varsa atla
            if (await HasOpenAlarmAsync(point.SensorCode, PlcSystemAlarmType.SensorTimeout, ct))
                continue;

            alarmSet.Add(new PlcSystemAlarm
            {
                Type = PlcSystemAlarmType.SensorTimeout,
                SensorCode = point.SensorCode,
                DevicePrefix = point.DevicePrefix,
                Message = $"Sensör {point.SensorCode} ({point.DeviceName}) {timeoutMinutes} dakikadır veri göndermiyor.",
                RelatedDeviceAddress = point.ModbusHost,
                OccurredAtUtc = DateTime.UtcNow,
                IsResolved = false,
            });
        }

        await Context.SaveChangesAsync(ct);
    }

    public async Task CreateModbusConnectionAlarmAsync(
        string devicePrefix,
        string? address,
        string message,
        CancellationToken ct = default)
    {
        // Aynı cihaz için açık bağlantı hatası alarmı varsa atla
        if (await Context.Set<PlcSystemAlarm>().AnyAsync(a => a.DevicePrefix == devicePrefix && a.Type == PlcSystemAlarmType.ModbusConnectionFailed && !a.IsResolved, ct))
            return;

        Context.Set<PlcSystemAlarm>().Add(new PlcSystemAlarm
        {
            Type = PlcSystemAlarmType.ModbusConnectionFailed,
            DevicePrefix = devicePrefix,
            Message = message,
            RelatedDeviceAddress = address,
            OccurredAtUtc = DateTime.UtcNow,
            IsResolved = false,
        });

        await Context.SaveChangesAsync(ct);
    }
}