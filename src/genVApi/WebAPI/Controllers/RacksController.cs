using Application.Features.Racks.Commands.Create;
using Application.Features.Racks.Commands.CreateRange;
using Application.Features.Racks.Commands.Delete;
using Application.Features.Racks.Commands.DeleteRange;
using Application.Features.Racks.Commands.Update;
using Application.Features.Racks.Commands.UpdateRange;
using Application.Features.Racks.Queries.GetById;
using Application.Features.Racks.Queries.GetList;
using Application.Features.Racks.Queries.GetListByDynamic;
using NArchitecture.Core.Application.Requests;
using NArchitecture.Core.Application.Responses;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class RacksController : BaseController
{
    [HttpPost]
    public async Task<IActionResult> Add([FromBody] CreateRackCommand createRackCommand)
    {
        CreatedRackResponse response = await Mediator.Send(createRackCommand);

        return Created(uri: "", response);
    }

    [HttpPut]
    public async Task<IActionResult> Update([FromBody] UpdateRackCommand updateRackCommand)
    {
        UpdatedRackResponse response = await Mediator.Send(updateRackCommand);

        return Ok(response);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete([FromRoute] Guid id)
    {
        DeletedRackResponse response = await Mediator.Send(new DeleteRackCommand { Id = id });

        return Ok(response);
    }

    [HttpPost("range")]
    public async Task<IActionResult> AddRange([FromBody] CreateRackRangeCommand createRackRangeCommand)
    {
        CreatedRackRangeResponse response = await Mediator.Send(createRackRangeCommand);

        return Created(uri: "", response);
    }

    [HttpPut("range")]
    public async Task<IActionResult> UpdateRange([FromBody] UpdateRackRangeCommand updateRackRangeCommand)
    {
        UpdatedRackRangeResponse response = await Mediator.Send(updateRackRangeCommand);

        return Ok(response);
    }

    [HttpPost("delete-range")]
    public async Task<IActionResult> DeleteRange([FromBody] DeleteRackRangeCommand deleteRackRangeCommand)
    {
        DeletedRackRangeResponse response = await Mediator.Send(deleteRackRangeCommand);

        return Ok(response);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById([FromRoute] Guid id)
    {
        GetByIdRackResponse response = await Mediator.Send(new GetByIdRackQuery { Id = id });
        return Ok(response);
    }

    [HttpGet]
    public async Task<IActionResult> GetList([FromQuery] PageRequest pageRequest)
    {
        GetListRackQuery getListRackQuery = new() { PageRequest = pageRequest };
        GetListResponse<GetListRackListItemDto> response = await Mediator.Send(getListRackQuery);
        return Ok(response);
    }

    [HttpPost("by-dynamic")]
    public async Task<IActionResult> GetListByDynamic([FromBody] GetListByDynamicRackQuery query)
    {
        GetListResponse<GetListRackListItemDto> response = await Mediator.Send(query);
        return Ok(response);
    }
}