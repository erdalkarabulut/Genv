using Application.Services.SmsService;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Sms;

/// <summary>SMS kapalı veya sağlayıcı seçilmemiş.</summary>
public sealed class NullSmsSender : ISmsSender
{
    private readonly ILogger<NullSmsSender>? _logger;

    public NullSmsSender(ILogger<NullSmsSender>? logger = null) => _logger = logger;

    public Task<SmsSendResult> SendAsync(string phoneNumber, string message, CancellationToken cancellationToken = default)
    {
        _logger?.LogWarning("SMS gönderilmedi (SmsSettings.Enabled=false veya Provider=none).");
        return Task.FromResult(
            new SmsSendResult(false, null, "SMS devre dışı. appsettings içinde SmsSettings.Enabled ve Provider yapılandırın."));
    }
}
