using Application.Features.PlcIntegration.Commands.ReportModbusFailure;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class PlcAlarmsController : BaseController
{
    /// <summary>PlcModbusWorker Modbus bağlantısı başarısız olduğunda bu endpointi çağırır.</summary>
    [HttpPost("report-modbus-failure")]
    public async Task<IActionResult> ReportModbusFailure([FromBody] ReportModbusFailureRequest request, CancellationToken ct)
    {
        await Mediator.Send(new ReportModbusFailureCommand
        {
            DevicePrefix = request.DevicePrefix,
            Address = request.Address,
            Message = request.Message,
        }, ct);
        return Ok();
    }
}

public class ReportModbusFailureRequest
{
    public string DevicePrefix { get; set; } = "";
    public string? Address { get; set; }
    public string Message { get; set; } = "";
}