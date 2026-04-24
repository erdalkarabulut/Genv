using System.Text.Json.Serialization;

namespace Licensing.Shared;

public sealed class OfflineLicenseDocument
{
    [JsonPropertyName("signedDocument")]
    public required string SignedDocument { get; set; }

    [JsonPropertyName("signatureBase64")]
    public required string SignatureBase64 { get; set; }
}
