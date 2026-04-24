using Application.Features.PlcIntegration.Commands.IngestTelemetry;
using Application.Features.PlcIntegration.Dtos;
using Application.Features.PlcIntegration.Queries.GetSyncConfig;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace WebAPI.Controllers;

/// <summary>Ayrı Modbus EXE ile konuşur: konfig çekme ve telemetri gönderme.</summary>
[Route("api/plc-integration")]
[ApiController]
public class PlcIntegrationController : BaseController
{
    private readonly IOptions<IndustrialIntegrationSettings> _integration;

    public PlcIntegrationController(IOptions<IndustrialIntegrationSettings> integration) =>
        _integration = integration;

    /// <summary>Worker tüm sensör satırlarını ve Modbus parametrelerini çeker.</summary>
    [HttpGet("sync")]
    public async Task<IActionResult> GetSync(CancellationToken cancellationToken)
    {
        IActionResult? deny = EnsureIndustrialApiKey();
        if (deny != null)
            return deny;

        List<PlcSensorSyncDto> result = await Mediator.Send(new GetPlcSyncConfigQuery(), cancellationToken);
        return Ok(result);
    }

    /// <summary>PLC'den okunan ölçümleri yazar; eşik ihlali ve bildirim kontağı varsa SMS tetiklenir.</summary>
    [HttpPost("telemetry")]
    public async Task<IActionResult> PostTelemetry([FromBody] IngestPlcTelemetryCommand command, CancellationToken cancellationToken)
    {
        IActionResult? deny = EnsureIndustrialApiKey();
        if (deny != null)
            return deny;

        IngestPlcTelemetryResponse result = await Mediator.Send(command, cancellationToken);
        return Ok(result);
    }

    private IActionResult? EnsureIndustrialApiKey()
    {
        string? expected = _integration.Value.ApiKey;
        if (string.IsNullOrWhiteSpace(expected))
            return StatusCode(StatusCodes.Status503ServiceUnavailable, new { message = "IndustrialIntegration.ApiKey yapılandırılmadı." });

        if (!Request.Headers.TryGetValue("X-Industrial-ApiKey", out Microsoft.Extensions.Primitives.StringValues hv)
            || hv.ToString() != expected)
            return Unauthorized(new { message = "Geçersiz veya eksik X-Industrial-ApiKey." });

        return null;
    }
}
