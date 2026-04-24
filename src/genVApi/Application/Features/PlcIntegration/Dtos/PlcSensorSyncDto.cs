namespace Application.Features.PlcIntegration.Dtos;

/// <summary>Modbus worker tarafından kullanılacak senkron satırı.</summary>
public sealed class PlcSensorSyncDto
{
    public Guid Id { get; set; }
    public string SensorCode { get; set; } = "";
    public string DeviceName { get; set; } = "";
    public string DevicePrefix { get; set; } = "";
    public string DataLabel { get; set; } = "";
    public string ModbusHost { get; set; } = "";
    public int ModbusPort { get; set; }
    public int SlaveId { get; set; }
    public int RegisterAddress { get; set; }
    public int RegisterLength { get; set; }
    public double ScaleDivisor { get; set; }
    public int PollIntervalSeconds { get; set; }
}
