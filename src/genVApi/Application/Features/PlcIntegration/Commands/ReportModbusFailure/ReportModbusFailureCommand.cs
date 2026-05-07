using Application.Features.PlcIntegration.Alarms;
using Application.Services.Repositories;
using MediatR;

namespace Application.Features.PlcIntegration.Commands.ReportModbusFailure;

public class ReportModbusFailureCommand : IRequest<Unit>
{
    public string DevicePrefix { get; set; } = "";
    public string? Address { get; set; }
    public string Message { get; set; } = "";
}

public class ReportModbusFailureHandler : IRequestHandler<ReportModbusFailureCommand, Unit>
{
    private readonly IPlcSystemAlarmRepository _alarms;

    public ReportModbusFailureHandler(IPlcSystemAlarmRepository alarms) => _alarms = alarms;

    public async Task<Unit> Handle(ReportModbusFailureCommand request, CancellationToken ct)
    {
        await _alarms.CreateModbusConnectionAlarmAsync(
            request.DevicePrefix,
            request.Address,
            request.Message,
            ct);
        return Unit.Value;
    }
}