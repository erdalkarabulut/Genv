using NArchitecture.Core.Persistence.Repositories;

namespace Domain.Entities;

public class PlcTelemetryReading : Entity<Guid>
{
    public Guid SensorPointId { get; set; }

    public virtual PlcSensorPoint SensorPoint { get; set; } = default!;

    public double Value { get; set; }

    public DateTime ReadAtUtc { get; set; }

    /// <summary>Ham register (isteğe bağlı iz)</summary>
    public int? RawRegisterValue { get; set; }
}
