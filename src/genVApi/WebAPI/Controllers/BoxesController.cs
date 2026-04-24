using Application.Features.Boxes.Commands.Create;
using Application.Features.Boxes.Commands.CreateRange;
using Application.Features.Boxes.Commands.Delete;
using Application.Features.Boxes.Commands.DeleteRange;
using Application.Features.Boxes.Commands.Update;
using Application.Features.Boxes.Commands.UpdateRange;
using Application.Features.Boxes.Queries.GetById;
using Application.Features.Boxes.Queries.GetList;
using Application.Features.Boxes.Queries.GetListByDynamic;
using NArchitecture.Core.Application.Requests;
using NArchitecture.Core.Application.Responses;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class BoxesController : BaseController
{
    [HttpPost]
    public async Task<IActionResult> Add([FromBody] CreateBoxCommand createBoxCommand)
    {
        CreatedBoxResponse response = await Mediator.Send(createBoxCommand);

        return Created(uri: "", response);
    }

    [HttpPut]
    public async Task<IActionResult> Update([FromBody] UpdateBoxCommand updateBoxCommand)
    {
        UpdatedBoxResponse response = await Mediator.Send(updateBoxCommand);

        return Ok(response);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete([FromRoute] Guid id)
    {
        DeletedBoxResponse response = await Mediator.Send(new DeleteBoxCommand { Id = id });

        return Ok(response);
    }

    [HttpPost("range")]
    public async Task<IActionResult> AddRange([FromBody] CreateBoxRangeCommand createBoxRangeCommand)
    {
        CreatedBoxRangeResponse response = await Mediator.Send(createBoxRangeCommand);

        return Created(uri: "", response);
    }

    [HttpPut("range")]
    public async Task<IActionResult> UpdateRange([FromBody] UpdateBoxRangeCommand updateBoxRangeCommand)
    {
        UpdatedBoxRangeResponse response = await Mediator.Send(updateBoxRangeCommand);

        return Ok(response);
    }

    [HttpPost("delete-range")]
    public async Task<IActionResult> DeleteRange([FromBody] DeleteBoxRangeCommand deleteBoxRangeCommand)
    {
        DeletedBoxRangeResponse response = await Mediator.Send(deleteBoxRangeCommand);

        return Ok(response);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById([FromRoute] Guid id)
    {
        GetByIdBoxResponse response = await Mediator.Send(new GetByIdBoxQuery { Id = id });
        return Ok(response);
    }

    [HttpGet]
    public async Task<IActionResult> GetList([FromQuery] PageRequest pageRequest)
    {
        GetListBoxQuery getListBoxQuery = new() { PageRequest = pageRequest };
        GetListResponse<GetListBoxListItemDto> response = await Mediator.Send(getListBoxQuery);
        return Ok(response);
    }

    [HttpPost("by-dynamic")]
    public async Task<IActionResult> GetListByDynamic([FromBody] GetListByDynamicBoxQuery query)
    {
        GetListResponse<GetListBoxListItemDto> response = await Mediator.Send(query);
        return Ok(response);
    }
}