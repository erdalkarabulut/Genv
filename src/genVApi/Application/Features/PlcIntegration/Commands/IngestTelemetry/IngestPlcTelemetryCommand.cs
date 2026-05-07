using System.Text.RegularExpressions;
using Application.Services.Repositories;
using Application.Services.SmsService;
using Domain.Entities;
using MediatR;
using NArchitecture.Core.Application.Pipelines.Logging;
using NArchitecture.Core.Persistence.Paging;

namespace Application.Features.PlcIntegration.Commands.IngestTelemetry;

public class IngestPlcTelemetryCommand : IRequest<IngestPlcTelemetryResponse>, ILoggableRequest
{
    public List<PlcTelemetryItemDto> Items { get; set; } = new();

    public class Handler : IRequestHandler<IngestPlcTelemetryCommand, IngestPlcTelemetryResponse>
    {
        private readonly IPlcSensorPointRepository _points;
        private readonly IPlcTelemetryReadingRepository _readings;
        private readonly IPlcAlarmContactRepository _contacts;
        private readonly IPlcAlarmTemplateRepository _templates;
        private readonly ISmsSender _sms;

        public Handler(
            IPlcSensorPointRepository points,
            IPlcTelemetryReadingRepository readings,
            IPlcAlarmContactRepository contacts,
            IPlcAlarmTemplateRepository templates,
            ISmsSender sms)
        {
            _points = points;
            _readings = readings;
            _contacts = contacts;
            _templates = templates;
            _sms = sms;
        }

        public async Task<IngestPlcTelemetryResponse> Handle(IngestPlcTelemetryCommand request, CancellationToken cancellationToken)
        {
            var response = new IngestPlcTelemetryResponse();

            IPaginate<PlcAlarmContact> contactPage = await _contacts.GetListAsync(
                predicate: null,
                orderBy: null,
                include: null,
                index: 0,
                size: int.MaxValue,
                enableTracking: false,
                cancellationToken: cancellationToken);
            List<PlcAlarmContact> allContacts = contactPage.Items.ToList();

            IPaginate<PlcAlarmTemplate> templatePage = await _templates.GetListAsync(
                predicate: t => t.IsActive,
                orderBy: null,
                include: null,
                index: 0,
                size: int.MaxValue,
                enableTracking: false,
                cancellationToken: cancellationToken);
            List<PlcAlarmTemplate> templates = templatePage.Items.ToList();

            foreach (PlcTelemetryItemDto item in request.Items)
            {
                PlcSensorPoint? point = await _points.GetAsync(p => p.SensorCode == item.SensorCode, cancellationToken: cancellationToken);
                if (point is null)
                {
                    response.SkippedUnknownCodes.Add(item.SensorCode);
                    continue;
                }

                IPaginate<PlcTelemetryReading> prevPage = await _readings.GetListAsync(
                    predicate: r => r.SensorPointId == point.Id,
                    orderBy: q => q.OrderByDescending(r => r.ReadAtUtc),
                    include: null,
                    index: 0,
                    size: 1,
                    enableTracking: false,
                    cancellationToken: cancellationToken);
                PlcTelemetryReading? previous = prevPage.Items.FirstOrDefault();

                DateTime readUtc = item.ReadAtUtc.Kind == DateTimeKind.Utc
                    ? item.ReadAtUtc
                    : item.ReadAtUtc.ToUniversalTime();

                var reading = new PlcTelemetryReading
                {
                    SensorPointId = point.Id,
                    Value = item.Value,
                    ReadAtUtc = readUtc,
                    RawRegisterValue = item.RawRegisterValue,
                };
                await _readings.AddAsync(reading);
                response.Recorded++;

                bool nowAlarm = IsInAlarm(point, item.Value);
                bool wasAlarm = previous is not null && IsInAlarm(point, previous.Value);

                if (!point.AlarmActive || !nowAlarm || wasAlarm)
                    continue;

                IEnumerable<PlcAlarmContact> recipients = allContacts.Where(c =>
                    string.IsNullOrEmpty(c.DevicePrefix) || c.DevicePrefix == point.DevicePrefix);

                foreach (PlcAlarmContact c in recipients.Where(x => x.SmsEnabled))
                {
                    string msg = ResolveTemplate(templates, c.AlarmTemplateId, point, item.Value);
                    SmsSendResult smsResult = await _sms.SendAsync(c.Phone, msg, cancellationToken);
                    if (smsResult.Success)
                        response.AlarmSmsSent++;
                }
            }

            return response;
        }

        private static string ResolveTemplate(List<PlcAlarmTemplate> templates, Guid? alarmTemplateId, PlcSensorPoint point, double value)
        {
            // 1) Kontakta özel template atanmışsa onu kullan
            if (alarmTemplateId.HasValue)
            {
                PlcAlarmTemplate? byContact = templates.FirstOrDefault(t => t.Id == alarmTemplateId.Value);
                if (byContact is not null)
                    return ApplyTemplate(byContact, point, value);
            }

            // 2) Yoksa cihaz önekine özel template'i dene
            PlcAlarmTemplate? byPrefix = templates
                .FirstOrDefault(t => t.DevicePrefix == point.DevicePrefix);

            // 3) Sonra varsayılan (boş prefix) template'i dene
            byPrefix ??= templates.FirstOrDefault(t => string.IsNullOrEmpty(t.DevicePrefix));

            if (byPrefix is null)
            {
                // Tamamen template yoksa eski hardcoded mesajı kullan
                return $"ALARM {point.DeviceName} / {point.DataLabel} ({point.SensorCode}) değer={value:0.###} " +
                       $"eşik düşük={point.AlarmLow} yüksek={point.AlarmHigh}";
            }

            return ApplyTemplate(byPrefix, point, value);
        }

        private static string ApplyTemplate(PlcAlarmTemplate template, PlcSensorPoint point, double value)
        {
            string msg = template.SmsTemplate;
            msg = msg.Replace("{DeviceName}", point.DeviceName);
            msg = msg.Replace("{DataLabel}", point.DataLabel);
            msg = msg.Replace("{SensorCode}", point.SensorCode);
            msg = msg.Replace("{Value}", value.ToString("0.###"));
            msg = msg.Replace("{AlarmLow}", point.AlarmLow?.ToString() ?? "-");
            msg = msg.Replace("{AlarmHigh}", point.AlarmHigh?.ToString() ?? "-");
            return msg;
        }

        private static bool IsInAlarm(PlcSensorPoint p, double v)
        {
            if (!p.AlarmActive)
                return false;
            if (p.AlarmLow.HasValue && v < p.AlarmLow.Value)
                return true;
            if (p.AlarmHigh.HasValue && v > p.AlarmHigh.Value)
                return true;
            return false;
        }
    }
}

public sealed class PlcTelemetryItemDto
{
    public string SensorCode { get; set; } = "";
    public double Value { get; set; }
    public DateTime ReadAtUtc { get; set; }
    public int? RawRegisterValue { get; set; }
}

public sealed class IngestPlcTelemetryResponse
{
    public int Recorded { get; set; }
    public int AlarmSmsSent { get; set; }
    public List<string> SkippedUnknownCodes { get; set; } = new();
}
