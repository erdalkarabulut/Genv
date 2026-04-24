using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Licensing.Shared;

public static class OfflineLicenseGuard
{
    public static void EnsureValidOrThrow(string contentRootPath, OfflineLicenseOptions opt)
    {
        if (!opt.Enabled)
            return;

        if (string.IsNullOrWhiteSpace(opt.PublicKeyPem))
            throw new InvalidOperationException(
                "OfflineLicense.Enabled ancak PublicKeyPem boş. Müşteri paketinde RSA public key PEM ekleyin.");

        string path = Path.IsPathRooted(opt.LicenseFilePath)
            ? opt.LicenseFilePath
            : Path.Combine(contentRootPath, opt.LicenseFilePath);

        if (!File.Exists(path))
            throw new InvalidOperationException(
                $"Lisans dosyası bulunamadı: {path}. Hedef makinede WebAPI --license-fingerprint çıktısını size iletin; ürettiğiniz .lic dosyasını bu yola koyun.");

        string json = File.ReadAllText(path);
        OfflineLicenseDocument? doc;
        try
        {
            doc = JsonSerializer.Deserialize<OfflineLicenseDocument>(json);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Lisans dosyası geçerli JSON değil.", ex);
        }

        if (doc is null || string.IsNullOrWhiteSpace(doc.SignedDocument) || string.IsNullOrWhiteSpace(doc.SignatureBase64))
            throw new InvalidOperationException("Lisans dosyasında signedDocument veya signatureBase64 eksik.");

        byte[] payloadBytes = Encoding.UTF8.GetBytes(doc.SignedDocument);
        byte[] signature = Convert.FromBase64String(doc.SignatureBase64);

        using RSA rsa = RSA.Create();
        rsa.ImportFromPem(opt.PublicKeyPem);

        bool ok = rsa.VerifyData(payloadBytes, signature, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        if (!ok)
            throw new InvalidOperationException("Lisans imzası geçersiz veya dosya değiştirilmiş.");

        OfflineLicensePayload? payload;
        try
        {
            payload = JsonSerializer.Deserialize<OfflineLicensePayload>(doc.SignedDocument);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("İmzalı lisans içeriği okunamadı.", ex);
        }

        if (payload is null)
            throw new InvalidOperationException("Lisans yükü boş.");

        if (!string.Equals(payload.ProductId, opt.ExpectedProductId, StringComparison.Ordinal))
            throw new InvalidOperationException(
                $"Ürün kimliği uyuşmuyor (beklenen: {opt.ExpectedProductId}, dosyada: {payload.ProductId}).");

        string machine = MachineFingerprintProvider.GetFingerprintSha256Hex();
        if (!string.Equals(payload.MachineFingerprintSha256, machine, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException(
                "Bu lisans bu makineye ait değil (parmak izi eşleşmiyor). Doğru makine için yeni lisans üretin.");
    }
}
