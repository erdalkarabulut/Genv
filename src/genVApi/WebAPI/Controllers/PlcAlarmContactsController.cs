using Application.Features.PlcIntegration.Commands.CreateAlarmContact;
using Application.Features.PlcIntegration.Commands.DeleteAlarmContact;
using Application.Features.PlcIntegration.Commands.UpdateAlarmContact;
using Application.Features.PlcIntegration.Queries.GetListPlcAlarmContacts;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers;

/// <summary>PLC alarm bildirim kontakları — yalnızca Admin (JWT).</summary>
[Route("api/[controller]")]
[ApiController]
public class PlcAlarmContactsController : BaseController
{
    [HttpGet]
    public async Task<IActionResult> GetList(CancellationToken cancellationToken)
    {
        List<GetListPlcAlarmContactListItemDto> result = await Mediator.Send(new GetListPlcAlarmContactsQuery(), cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreatePlcAlarmContactCommand command)
    {
        CreatedPlcAlarmContactResponse result = await Mediator.Send(command);
        return Created($"/api/PlcAlarmContacts/{result.Id}", result);
    }

    [HttpPut]
    public async Task<IActionResult> Update([FromBody] UpdatePlcAlarmContactCommand command)
    {
        UpdatedPlcAlarmContactResponse result = await Mediator.Send(command);
        return Ok(result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete([FromRoute] Guid id)
    {
        DeletedPlcAlarmContactResponse result = await Mediator.Send(new DeletePlcAlarmContactCommand { Id = id });
        return Ok(result);
    }
}
