using Application.Features.PlcIntegration.Commands.CreateSensorPoint;
using Application.Features.PlcIntegration.Commands.DeleteSensorPoint;
using Application.Features.PlcIntegration.Commands.UpdateSensorPoint;
using Application.Features.PlcIntegration.Queries.GetListPlcSensorPoints;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers;

/// <summary>Yönetim: sensör / Modbus tanımları (JWT Admin).</summary>
[Route("api/[controller]")]
[ApiController]
public class PlcSensorPointsController : BaseController
{
    [HttpGet]
    public async Task<IActionResult> GetList(CancellationToken cancellationToken)
    {
        List<GetListPlcSensorPointListItemDto> result = await Mediator.Send(new GetListPlcSensorPointsQuery(), cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreatePlcSensorPointCommand command)
    {
        CreatedPlcSensorPointResponse result = await Mediator.Send(command);
        return Created($"/api/PlcSensorPoints/{result.Id}", result);
    }

    [HttpPut]
    public async Task<IActionResult> Update([FromBody] UpdatePlcSensorPointCommand command)
    {
        UpdatedPlcSensorPointResponse result = await Mediator.Send(command);
        return Ok(result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete([FromRoute] Guid id)
    {
        DeletedPlcSensorPointResponse result = await Mediator.Send(new DeletePlcSensorPointCommand { Id = id });
        return Ok(result);
    }
}
