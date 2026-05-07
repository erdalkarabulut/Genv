using Application.Features.PlcIntegration.Commands.IngestTelemetry;
using Application.Features.PlcIntegration.Dtos;
using Application.Features.PlcIntegration.Queries.GetSyncConfig;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers;

/// <summary>Ayrı Modbus EXE ile konuşur: konfig çekme ve telemetri gönderme.</summary>
[Route("api/plc-integration")]
[ApiController]
public class PlcIntegrationController : BaseController
{
    /// <summary>Worker tüm sensör satırlarını ve Modbus parametrelerini çeker.</summary>
    [HttpGet("sync")]
    public async Task<IActionResult> GetSync(CancellationToken cancellationToken)
    {
        List<PlcSensorSyncDto> result = await Mediator.Send(new GetPlcSyncConfigQuery(), cancellationToken);
        return Ok(result);
    }

    /// <summary>PLC'den okunan ölçümleri yazar; eşik ihlali ve bildirim kontağı varsa SMS tetiklenir.</summary>
    [HttpPost("telemetry")]
    public async Task<IActionResult> PostTelemetry([FromBody] IngestPlcTelemetryCommand command, CancellationToken cancellationToken)
    {
        IngestPlcTelemetryResponse result = await Mediator.Send(command, cancellationToken);
        return Ok(result);
    }
}
