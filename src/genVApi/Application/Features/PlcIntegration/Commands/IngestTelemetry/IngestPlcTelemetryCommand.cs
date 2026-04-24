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
        private readonly ISmsSender _sms;

        public Handler(
            IPlcSensorPointRepository points,
            IPlcTelemetryReadingRepository readings,
            IPlcAlarmContactRepository contacts,
            ISmsSender sms)
        {
            _points = points;
            _readings = readings;
            _contacts = contacts;
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

                string msg =
                    $"ALARM {point.DeviceName} / {point.DataLabel} ({point.SensorCode}) değer={item.Value:0.###} " +
                    $"eşik düşük={point.AlarmLow} yüksek={point.AlarmHigh}";

                foreach (PlcAlarmContact c in recipients.Where(x => x.SmsEnabled))
                {
                    SmsSendResult smsResult = await _sms.SendAsync(c.Phone, msg, cancellationToken);
                    if (smsResult.Success)
                        response.AlarmSmsSent++;
                }
            }

            return response;
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
