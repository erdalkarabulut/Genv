namespace Licensing.Shared;

public sealed class OfflineLicenseOptions
{
    public const string SectionName = "OfflineLicense";

    public bool Enabled { get; set; }

    public string PublicKeyPem { get; set; } = "";

    public string LicenseFilePath { get; set; } = "license.genv.lic";

    public string ExpectedProductId { get; set; } = "GenVApi";
}
