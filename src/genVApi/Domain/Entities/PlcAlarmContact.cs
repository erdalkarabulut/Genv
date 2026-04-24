using NArchitecture.Core.Persistence.Repositories;

namespace Domain.Entities;

/// <summary>Kritik alarmda SMS/e-posta gidecek kişi (cihaz önekine göre).</summary>
public class PlcAlarmContact : Entity<Guid>
{
    /// <summary>Boş veya null = tüm cihazlar; aksi halde SensorPoint.DevicePrefix ile eşleşir (ör. BT01).</summary>
    public string? DevicePrefix { get; set; }

    public string DisplayName { get; set; } = default!;

    public string Phone { get; set; } = default!;

    public string? Email { get; set; }

    public bool SmsEnabled { get; set; }

    public bool EmailEnabled { get; set; }
}
