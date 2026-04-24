using NArchitecture.Core.Persistence.Repositories;

namespace Domain.Entities;

/// <summary>
/// Tank / hat vb. cihazlardaki tek bir Modbus ölçüm noktası (ör. BT01-S01 SEVİYE).
/// Cryosoft tarafındaki sensör satırına karşılık gelir.
/// </summary>
public class PlcSensorPoint : Entity<Guid>
{
    /// <summary>Benzersiz kod: BT01-S01</summary>
    public string SensorCode { get; set; } = default!;

    public string DeviceName { get; set; } = default!;

    /// <summary>Örn: BT01 — bildirim eşlemesi için prefix.</summary>
    public string DevicePrefix { get; set; } = default!;

    public string DataLabel { get; set; } = default!;

    public string ModbusHost { get; set; } = default!;

    public int ModbusPort { get; set; } = 502;

    public int SlaveId { get; set; }

    public int RegisterAddress { get; set; }

    public int RegisterLength { get; set; } = 1;

    /// <summary>Ham register / kesir — örn: 10 ise değer = raw/10</summary>
    public double ScaleDivisor { get; set; } = 1;

    public int PollIntervalSeconds { get; set; } = 5;

    public double? AlarmLow { get; set; }

    public double? AlarmHigh { get; set; }

    public bool AlarmActive { get; set; }

    public virtual ICollection<PlcTelemetryReading> Readings { get; set; } = new HashSet<PlcTelemetryReading>();
}
