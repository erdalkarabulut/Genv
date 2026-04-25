using Application.Features.RackSlots.Commands.Create;
using Application.Features.RackSlots.Commands.Delete;
using Application.Features.RackSlots.Commands.Update;
using Application.Features.RackSlots.Queries.GetById;
using Application.Features.RackSlots.Queries.GetList;
using Microsoft.AspNetCore.Mvc;
using NArchitecture.Core.Application.Requests;
using NArchitecture.Core.Application.Responses;

namespace WebAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class RackSlotsController : BaseController
{
    [HttpGet]
    public async Task<IActionResult> GetList([FromQuery] PageRequest pageRequest, [FromQuery] Guid? rackId)
    {
        GetListRackSlotQuery query = new() { PageRequest = pageRequest, RackId = rackId };
        GetListResponse<GetListRackSlotListItemDto> response = await Mediator.Send(query);
        return Ok(response);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById([FromRoute] Guid id)
    {
        GetByIdRackSlotResponse response = await Mediator.Send(new GetByIdRackSlotQuery { Id = id });
        return Ok(response);
    }

    [HttpPost]
    public async Task<IActionResult> Add([FromBody] CreateRackSlotCommand command)
    {
        CreatedRackSlotResponse response = await Mediator.Send(command);
        return Created(uri: "", response);
    }

    [HttpPut]
    public async Task<IActionResult> Update([FromBody] UpdateRackSlotCommand command)
    {
        UpdatedRackSlotResponse response = await Mediator.Send(command);
        return Ok(response);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete([FromRoute] Guid id)
    {
        DeletedRackSlotResponse response = await Mediator.Send(new DeleteRackSlotCommand { Id = id });
        return Ok(response);
    }
}
