using NArchitecture.Core.Persistence.Repositories;

namespace Domain.Entities;

/// <summary>
/// Alarm durumunda gönderilecek SMS/E-posta mesajının template'i.
/// Placeholder'lar: {DeviceName}, {DataLabel}, {SensorCode}, {Value}, {AlarmLow}, {AlarmHigh}
/// </summary>
public class PlcAlarmTemplate : Entity<Guid>
{
    public string Name { get; set; } = default!;

    /// <summary>
    /// SMS mesaj template'i. Placeholder: {DeviceName}, {DataLabel}, {SensorCode}, {Value}, {AlarmLow}, {AlarmHigh}
    /// </summary>
    public string SmsTemplate { get; set; } = default!;

    /// <summary>
    /// E-posta konu template'i. (isteğe bağlı)
    /// </summary>
    public string? EmailSubjectTemplate { get; set; }

    /// <summary>
    /// E-posta gövde template'i. (isteğe bağlı)
    /// </summary>
    public string? EmailBodyTemplate { get; set; }

    /// <summary>
    /// Bu template hangi cihaz öneki için geçerli? Boş = tümü için varsayılan.
    /// </summary>
    public string? DevicePrefix { get; set; }

    /// <summary>
    /// Aktif / pasif.
    /// </summary>
    public bool IsActive { get; set; } = true;
}
