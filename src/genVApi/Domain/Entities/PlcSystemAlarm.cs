using NArchitecture.Core.Persistence.Repositories;

namespace Domain.Entities;

/// <summary>
/// PLC sisteminin kendisiyle ilgili alarmlar — Modbus bağlantı hatası, sensör timeout gibi.
/// </summary>
public class PlcSystemAlarm : Entity<Guid>
{
    public PlcSystemAlarmType Type { get; set; }

    public string DevicePrefix { get; set; } = "";

    public string SensorCode { get; set; } = "";

    public string Message { get; set; } = "";

    /// <summary>Alarm oluştuğunda PLC'nin ID'si veya sensör kodu</summary>
    public string? RelatedDeviceAddress { get; set; }

    public DateTime OccurredAtUtc { get; set; } = DateTime.UtcNow;

    /// <summary>Sistemin otomatik iyileşip iyileşmediği (okundu işaretlenmesi)</summary>
    public bool IsResolved { get; set; }

    public DateTime? ResolvedAtUtc { get; set; }
}

public enum PlcSystemAlarmType
{
    /// <summary>Modbus TCP bağlantısı kurulamadı veya timeout oldu</summary>
    ModbusConnectionFailed = 1,

    /// <summary>Tanımlı sensörden belirlenen sürede veri gelmedi</summary>
    SensorTimeout = 2,

    /// <summary>Worker uygulaması çalışmıyor veya heartbeat almıyor</summary>
    WorkerNotRunning = 3,
}