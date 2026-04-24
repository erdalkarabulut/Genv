using System.Net;
using Application.Services.SmsService;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Sms;

public sealed class NetgsmSmsSender : ISmsSender
{
    private readonly HttpClient _http;
    private readonly SmsSettings _cfg;
    private readonly ILogger<NetgsmSmsSender> _logger;

    public NetgsmSmsSender(HttpClient http, SmsSettings cfg, ILogger<NetgsmSmsSender> logger)
    {
        _http = http;
        _cfg = cfg;
        _logger = logger;
    }

    public async Task<SmsSendResult> SendAsync(string phoneNumber, string message, CancellationToken cancellationToken = default)
    {
        string user = _cfg.NetgsmUserCode ?? throw new InvalidOperationException("SmsSettings.NetgsmUserCode eksik.");
        string pass = _cfg.NetgsmPassword ?? throw new InvalidOperationException("SmsSettings.NetgsmPassword eksik.");
        string header = _cfg.NetgsmMsgHeader ?? throw new InvalidOperationException("SmsSettings.NetgsmMsgHeader eksik.");

        string gsm = SmsPhoneNormalizer.ToTurkeyGsmDigits(phoneNumber);

        string url =
            "https://api.netgsm.com.tr/sms/send/get"
            + "?usercode=" + Uri.EscapeDataString(user)
            + "&password=" + Uri.EscapeDataString(pass)
            + "&gsmno=" + Uri.EscapeDataString(gsm)
            + "&message=" + Uri.EscapeDataString(message)
            + "&msgheader=" + Uri.EscapeDataString(header);

        HttpResponseMessage response = await _http.GetAsync(url, cancellationToken);
        string body = await response.Content.ReadAsStringAsync(cancellationToken);
        string trimmed = body.Trim();

        _logger.LogInformation("Netgsm yanıtı: {Status} {Body}", (int)response.StatusCode, trimmed);

        if (response.StatusCode != HttpStatusCode.OK)
            return new SmsSendResult(false, null, $"HTTP {(int)response.StatusCode}: {trimmed}");

        // Netgsm hata kodları çoğunlukla 2 haneli (ör. 20, 30, 40, 50, 51, 70, 85)
        if (trimmed.Length <= 3 && trimmed.All(char.IsDigit) && trimmed is not "00")
            return new SmsSendResult(false, null, $"Netgsm hata kodu: {trimmed}");

        return new SmsSendResult(true, trimmed, null);
    }
}
