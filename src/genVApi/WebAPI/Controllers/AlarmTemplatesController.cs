using Application.Features.PlcIntegration.Commands.CreateAlarmTemplate;
using Application.Features.PlcIntegration.Commands.DeleteAlarmTemplate;
using Application.Features.PlcIntegration.Commands.UpdateAlarmTemplate;
using Application.Features.PlcIntegration.Queries.GetListAlarmTemplate;
using Microsoft.AspNetCore.Mvc;
using NArchitecture.Core.Application.Requests;

namespace WebAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AlarmTemplatesController : BaseController
{
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] PageRequest pageRequest, CancellationToken cancellationToken)
    {
        var query = new GetListAlarmTemplateQuery { PageRequest = pageRequest };
        var result = await Mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreatePlcAlarmTemplateCommand command, CancellationToken cancellationToken)
    {
        CreatedPlcAlarmTemplateResponse result = await Mediator.Send(command, cancellationToken);
        return Created("", result);
    }

    [HttpPut]
    public async Task<IActionResult> Update([FromBody] UpdatePlcAlarmTemplateCommand command, CancellationToken cancellationToken)
    {
        UpdatedPlcAlarmTemplateResponse result = await Mediator.Send(command, cancellationToken);
        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        DeletedPlcAlarmTemplateResponse result = await Mediator.Send(new DeletePlcAlarmTemplateCommand { Id = id }, cancellationToken);
        return Ok(result);
    }
}
