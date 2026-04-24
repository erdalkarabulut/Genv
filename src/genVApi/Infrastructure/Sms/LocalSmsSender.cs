using Application.Services.SmsService;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Sms;

/// <summary>
/// Ücretsiz yerel/deneme modu: gerçek SMS göndermez; üretimde ücretli sağlayıcı (Netgsm vb.) kullanılmalıdır.
/// API ve test akışları ücretsiz doğrulanır.
/// </summary>
public sealed class LocalSmsSender : ISmsSender
{
    private readonly ILogger<LocalSmsSender> _logger;

    public LocalSmsSender(ILogger<LocalSmsSender> logger) => _logger = logger;

    public Task<SmsSendResult> SendAsync(string phoneNumber, string message, CancellationToken cancellationToken = default)
    {
        string normalized = SmsPhoneNormalizer.ToTurkeyGsmDigits(phoneNumber);
        string preview = message.Length > 160 ? message[..160] + "…" : message;

        _logger.LogInformation(
            "[SMS yerel/mock] Gönderilmiyor (ücretsiz mod). No: {Phone} Normalize: {Norm} Mesaj: {Preview}",
            phoneNumber,
            normalized,
            preview);

        return Task.FromResult(new SmsSendResult(true, $"local-{Guid.NewGuid():N}", null));
    }
}
