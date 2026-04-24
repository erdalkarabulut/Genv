namespace Infrastructure.Sms;

public static class SmsPhoneNormalizer
{
    /// <summary>Türkiye GSM için Netgsm uyumu: 905XXXXXXXXX (sadece rakam).</summary>
    public static string ToTurkeyGsmDigits(string raw)
    {
        Span<char> buf = stackalloc char[24];
        int n = 0;
        foreach (char c in raw)
        {
            if (char.IsAsciiDigit(c) && n < buf.Length)
                buf[n++] = c;
        }

        ReadOnlySpan<char> d = buf[..n];

        if (d.Length >= 12 && d.StartsWith("90", StringComparison.Ordinal))
            return new string(d[..12]);

        if (d.Length == 11 && d[0] == '0')
            return string.Concat("90", new string(d.Slice(1)));

        if (d.Length == 10 && d[0] == '5')
            return string.Concat("90", new string(d));

        return new string(d);
    }

    /// <summary>Twilio için +E164 (varsayılan TR bağlamında).</summary>
    public static string ToTurkeyTwilioE164(string raw)
        => "+" + ToTurkeyGsmDigits(raw).TrimStart('+');
}
