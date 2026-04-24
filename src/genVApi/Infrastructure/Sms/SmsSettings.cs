namespace Infrastructure.Sms;

public sealed class SmsSettings
{
    public const string SectionName = "SmsSettings";

    /// <summary>True iken sağlayıcıdan gönderim yapılır; false iken gönderim yapılmaz (log / hata mesajı).</summary>
    public bool Enabled { get; set; }

    /// <summary>
    /// none — kapalı. local — ücretsiz mock (gerçek SMS yok). netgsm, twilio — ücretli operatör.
    /// </summary>
    public string Provider { get; set; } = "none";

    public string? NetgsmUserCode { get; set; }
    public string? NetgsmPassword { get; set; }

    /// <summary>Netgsm başlık (operator onayı ile tanımlı gönderici adı).</summary>
    public string? NetgsmMsgHeader { get; set; }

    public string? TwilioAccountSid { get; set; }
    public string? TwilioAuthToken { get; set; }

    /// <summary>E.164 örn +905551234567 — Twilio From.</summary>
    public string? TwilioFromNumber { get; set; }
}
