using System.Security.Cryptography;
using System.Text.Json;
using Licensing.Shared;

/// <summary>
/// Müşteri makineden alınan parmak izi (--license-fingerprint) ile offline .lic üretir.
/// Özel RSA anahtarı SADECE sizde kalır; müşteri paketine yalnızca public key (appsettings) konur.
/// Örnek:
/// dotnet run --project tools/LicenseIssuer -- --private-key rsa_private.pem --fingerprint ABC123... --out license.genv.lic
/// </summary>
internal static class Program
{
    private static int Main(string[] args)
    {
        try
        {
            return Run(args);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine(ex.Message);
            return 1;
        }
    }

    private static int Run(string[] args)
    {
        string? keyPath = Arg(args, "--private-key");
        string? fingerprint = Arg(args, "--fingerprint");
        string? outPath = Arg(args, "--out") ?? "license.genv.lic";
        string? customer = Arg(args, "--customer");
        string? tenant = Arg(args, "--tenant");
        string productId = Arg(args, "--product-id") ?? "GenVApi";

        if (string.IsNullOrEmpty(keyPath) || string.IsNullOrEmpty(fingerprint))
        {
            Console.WriteLine("""
                Kullanım:
                  LicenseIssuer --private-key <rsa_private.pem> --fingerprint <hex> [--out file.lic] [--customer metin] [--tenant kod] [--product-id GenVApi]

                Parmak izi, hedef makinede WebAPI'nin --license-fingerprint çıktısıdır.
                """);
            return 1;
        }

        string pem = File.ReadAllText(keyPath);
        using RSA rsa = RSA.Create();
        rsa.ImportFromPem(pem);

        var payload = new OfflineLicensePayload
        {
            ProductId = productId,
            MachineFingerprintSha256 = fingerprint.Trim().ToLowerInvariant(),
            IssuedUtc = DateTime.UtcNow,
            CustomerLabel = customer,
            TenantTag = tenant,
        };

        string signedDocument = JsonSerializer.Serialize(payload, OfflineLicensePayload.SerializerOptions);
        byte[] sig = rsa.SignData(System.Text.Encoding.UTF8.GetBytes(signedDocument), HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);

        var doc = new OfflineLicenseDocument
        {
            SignedDocument = signedDocument,
            SignatureBase64 = Convert.ToBase64String(sig),
        };

        string licJson = JsonSerializer.Serialize(doc, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(outPath, licJson);
        Console.WriteLine($"Yazıldı: {Path.GetFullPath(outPath)}");
        return 0;
    }

    private static string? Arg(string[] args, string name)
    {
        for (int i = 0; i < args.Length - 1; i++)
            if (string.Equals(args[i], name, StringComparison.OrdinalIgnoreCase))
                return args[i + 1];
        return null;
    }
}
