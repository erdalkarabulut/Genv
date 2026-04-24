namespace Application.Services.SmsService;

/// <summary>
/// SMS sağlayıcı entegrasyonu (Netgsm, Twilio vb.). Uygulama içinden tek giriş noktası.
/// </summary>
public interface ISmsSender
{
    /// <param name="phoneNumber">Ham veya normalize edilmemiş numara; uygulama tarafında TR için normalize edilir.</param>
    Task<SmsSendResult> SendAsync(string phoneNumber, string message, CancellationToken cancellationToken = default);
}
