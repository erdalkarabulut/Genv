using System;
using Application.Services.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Application.Features.PlcIntegration.Alarms;

/// <summary>
/// Arka planda çalışan servis: sensörlerin en son veri gönderme zamanını kontrol eder.
/// Belirlenen sürede veri göndermeyen sensörler için PlcSystemAlarm (SensorTimeout) oluşturur.
/// </summary>
public class SensorTimeoutMonitorService : BackgroundService
{
    private readonly IServiceProvider _sp;
    private readonly ILogger<SensorTimeoutMonitorService> _log;
    private readonly TimeSpan _checkInterval = TimeSpan.FromMinutes(2);

    public SensorTimeoutMonitorService(IServiceProvider sp, ILogger<SensorTimeoutMonitorService> log)
    {
        _sp = sp;
        _log = log;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _log.LogInformation("SensorTimeoutMonitorService başladı");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _sp.CreateScope();
                var repo = scope.ServiceProvider.GetRequiredService<IPlcSystemAlarmRepository>();
                // 5 dakikadır veri göndermeyen sensörler için alarm oluştur
                await repo.CheckAndCreateSensorTimeoutAlarmsAsync(timeoutMinutes: 5, stoppingToken);
            }
            catch (OperationCanceledException) { break; }
            catch (Exception ex)
            {
                _log.LogError(ex, "SensorTimeoutMonitorService hatası");
            }

            await Task.Delay(_checkInterval, stoppingToken);
        }
    }
}