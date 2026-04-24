using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Application.Services.SmsService;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Sms;

public sealed class TwilioSmsSender : ISmsSender
{
    private readonly HttpClient _http;
    private readonly SmsSettings _cfg;
    private readonly ILogger<TwilioSmsSender> _logger;

    public TwilioSmsSender(HttpClient http, SmsSettings cfg, ILogger<TwilioSmsSender> logger)
    {
        _http = http;
        _cfg = cfg;
        _logger = logger;
    }

    public async Task<SmsSendResult> SendAsync(string phoneNumber, string message, CancellationToken cancellationToken = default)
    {
        string sid = _cfg.TwilioAccountSid ?? throw new InvalidOperationException("SmsSettings.TwilioAccountSid eksik.");
        string token = _cfg.TwilioAuthToken ?? throw new InvalidOperationException("SmsSettings.TwilioAuthToken eksik.");
        string from = _cfg.TwilioFromNumber ?? throw new InvalidOperationException("SmsSettings.TwilioFromNumber eksik.");

        string to = SmsPhoneNormalizer.ToTurkeyTwilioE164(phoneNumber);

        string auth = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{sid}:{token}"));
        using var req = new HttpRequestMessage(
            HttpMethod.Post,
            $"https://api.twilio.com/2010-04-01/Accounts/{sid}/Messages.json");
        req.Headers.Authorization = new AuthenticationHeaderValue("Basic", auth);
        req.Content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["To"] = to,
            ["From"] = from,
            ["Body"] = message,
        });

        HttpResponseMessage response = await _http.SendAsync(req, cancellationToken);
        string body = await response.Content.ReadAsStringAsync(cancellationToken);
        _logger.LogInformation("Twilio yanıtı: {Status} {Body}", (int)response.StatusCode, body);

        if (!response.IsSuccessStatusCode)
            return new SmsSendResult(false, null, body);

        try
        {
            using JsonDocument doc = JsonDocument.Parse(body);
            string? messageSid = doc.RootElement.TryGetProperty("sid", out JsonElement el) ? el.GetString() : null;
            return new SmsSendResult(true, messageSid, null);
        }
        catch
        {
            return new SmsSendResult(true, null, null);
        }
    }
}
