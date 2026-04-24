using Application.Features.Slots.Commands.Create;
using Application.Features.Slots.Commands.CreateRange;
using Application.Features.Slots.Commands.Delete;
using Application.Features.Slots.Commands.DeleteRange;
using Application.Features.Slots.Commands.Update;
using Application.Features.Slots.Commands.UpdateRange;
using Application.Features.Slots.Queries.GetById;
using Application.Features.Slots.Queries.GetList;
using Application.Features.Slots.Queries.GetListByDynamic;
using NArchitecture.Core.Application.Requests;
using NArchitecture.Core.Application.Responses;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class SlotsController : BaseController
{
    [HttpPost]
    public async Task<IActionResult> Add([FromBody] CreateSlotCommand createSlotCommand)
    {
        CreatedSlotResponse response = await Mediator.Send(createSlotCommand);

        return Created(uri: "", response);
    }

    [HttpPut]
    public async Task<IActionResult> Update([FromBody] UpdateSlotCommand updateSlotCommand)
    {
        UpdatedSlotResponse response = await Mediator.Send(updateSlotCommand);

        return Ok(response);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete([FromRoute] Guid id)
    {
        DeletedSlotResponse response = await Mediator.Send(new DeleteSlotCommand { Id = id });

        return Ok(response);
    }

    [HttpPost("range")]
    public async Task<IActionResult> AddRange([FromBody] CreateSlotRangeCommand createSlotRangeCommand)
    {
        CreatedSlotRangeResponse response = await Mediator.Send(createSlotRangeCommand);

        return Created(uri: "", response);
    }

    [HttpPut("range")]
    public async Task<IActionResult> UpdateRange([FromBody] UpdateSlotRangeCommand updateSlotRangeCommand)
    {
        UpdatedSlotRangeResponse response = await Mediator.Send(updateSlotRangeCommand);

        return Ok(response);
    }

    [HttpPost("delete-range")]
    public async Task<IActionResult> DeleteRange([FromBody] DeleteSlotRangeCommand deleteSlotRangeCommand)
    {
        DeletedSlotRangeResponse response = await Mediator.Send(deleteSlotRangeCommand);

        return Ok(response);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById([FromRoute] Guid id)
    {
        GetByIdSlotResponse response = await Mediator.Send(new GetByIdSlotQuery { Id = id });
        return Ok(response);
    }

    [HttpGet]
    public async Task<IActionResult> GetList([FromQuery] PageRequest pageRequest)
    {
        GetListSlotQuery getListSlotQuery = new() { PageRequest = pageRequest };
        GetListResponse<GetListSlotListItemDto> response = await Mediator.Send(getListSlotQuery);
        return Ok(response);
    }

    [HttpPost("by-dynamic")]
    public async Task<IActionResult> GetListByDynamic([FromBody] GetListByDynamicSlotQuery query)
    {
        GetListResponse<GetListSlotListItemDto> response = await Mediator.Send(query);
        return Ok(response);
    }
}