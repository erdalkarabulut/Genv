namespace WebAPI;

/// <summary>PLC / Modbus worker için makine kimlik doğrulaması.</summary>
public sealed class IndustrialIntegrationSettings
{
    public const string SectionName = "IndustrialIntegration";

    /// <summary>X-Industrial-ApiKey başlığı ile gönderilir. Boşsa plc-integration endpoint'leri çalışmaz.</summary>
    public string ApiKey { get; set; } = "wsx53ujmwsx53ujm";
}
