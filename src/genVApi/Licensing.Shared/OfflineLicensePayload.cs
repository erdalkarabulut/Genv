using System.Text.Json;
using System.Text.Json.Serialization;

namespace Licensing.Shared;

public sealed class OfflineLicensePayload
{
    [JsonPropertyOrder(0)]
    [JsonPropertyName("productId")]
    public string ProductId { get; set; } = "GenVApi";

    [JsonPropertyOrder(1)]
    [JsonPropertyName("machineFingerprintSha256")]
    public required string MachineFingerprintSha256 { get; set; }

    [JsonPropertyOrder(2)]
    [JsonPropertyName("issuedUtc")]
    public DateTime IssuedUtc { get; set; }

    [JsonPropertyOrder(3)]
    [JsonPropertyName("customerLabel")]
    public string? CustomerLabel { get; set; }

    [JsonPropertyOrder(4)]
    [JsonPropertyName("tenantTag")]
    public string? TenantTag { get; set; }

    public static JsonSerializerOptions SerializerOptions { get; } = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };
}
