using Application.Services.SmsService;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Infrastructure.Sms;

public static class SmsInfrastructureRegistration
{
    public static IServiceCollection AddSmsServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<SmsSettings>().Bind(configuration.GetSection(SmsSettings.SectionName));

        services.AddHttpClient(nameof(NetgsmSmsSender));
        services.AddHttpClient(nameof(TwilioSmsSender));

        services.AddSingleton<ISmsSender>(sp =>
        {
            SmsSettings cfg = sp.GetRequiredService<IOptions<SmsSettings>>().Value;
            IHttpClientFactory factory = sp.GetRequiredService<IHttpClientFactory>();
            ILoggerFactory lf = sp.GetRequiredService<ILoggerFactory>();

            if (!cfg.Enabled || string.Equals(cfg.Provider, "none", StringComparison.OrdinalIgnoreCase))
                return new NullSmsSender(lf.CreateLogger<NullSmsSender>());

            string p = cfg.Provider.Trim().ToUpperInvariant();
            return p switch
            {
                "LOCAL" or "MOCK" or "FREE" or "DEV" or "DEVELOPMENT"
                    => new LocalSmsSender(lf.CreateLogger<LocalSmsSender>()),
                "NETGSM" => new NetgsmSmsSender(
                    factory.CreateClient(nameof(NetgsmSmsSender)),
                    cfg,
                    lf.CreateLogger<NetgsmSmsSender>()),
                "TWILIO" => new TwilioSmsSender(
                    factory.CreateClient(nameof(TwilioSmsSender)),
                    cfg,
                    lf.CreateLogger<TwilioSmsSender>()),
                _ => throw new InvalidOperationException(
                    $"SmsSettings.Provider bilinmiyor: {cfg.Provider}. none | local (ücretsiz mock) | netgsm | twilio kullanın."),
            };
        });

        return services;
    }
}
