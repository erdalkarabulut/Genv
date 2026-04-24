using Application.Features.Tanks.Commands.Create;
using Application.Features.Tanks.Commands.CreateRange;
using Application.Features.Tanks.Commands.Delete;
using Application.Features.Tanks.Commands.DeleteRange;
using Application.Features.Tanks.Commands.Update;
using Application.Features.Tanks.Commands.UpdateRange;
using Application.Features.Tanks.Queries.GetById;
using Application.Features.Tanks.Queries.GetList;
using Application.Features.Tanks.Queries.GetListByDynamic;
using NArchitecture.Core.Application.Requests;
using NArchitecture.Core.Application.Responses;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class TanksController : BaseController
{
    [HttpPost]
    public async Task<IActionResult> Add([FromBody] CreateTankCommand createTankCommand)
    {
        CreatedTankResponse response = await Mediator.Send(createTankCommand);

        return Created(uri: "", response);
    }

    [HttpPut]
    public async Task<IActionResult> Update([FromBody] UpdateTankCommand updateTankCommand)
    {
        UpdatedTankResponse response = await Mediator.Send(updateTankCommand);

        return Ok(response);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete([FromRoute] Guid id)
    {
        DeletedTankResponse response = await Mediator.Send(new DeleteTankCommand { Id = id });

        return Ok(response);
    }

    [HttpPost("range")]
    public async Task<IActionResult> AddRange([FromBody] CreateTankRangeCommand createTankRangeCommand)
    {
        CreatedTankRangeResponse response = await Mediator.Send(createTankRangeCommand);

        return Created(uri: "", response);
    }

    [HttpPut("range")]
    public async Task<IActionResult> UpdateRange([FromBody] UpdateTankRangeCommand updateTankRangeCommand)
    {
        UpdatedTankRangeResponse response = await Mediator.Send(updateTankRangeCommand);

        return Ok(response);
    }

    [HttpPost("delete-range")]
    public async Task<IActionResult> DeleteRange([FromBody] DeleteTankRangeCommand deleteTankRangeCommand)
    {
        DeletedTankRangeResponse response = await Mediator.Send(deleteTankRangeCommand);

        return Ok(response);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById([FromRoute] Guid id)
    {
        GetByIdTankResponse response = await Mediator.Send(new GetByIdTankQuery { Id = id });
        return Ok(response);
    }

    [HttpGet]
    public async Task<IActionResult> GetList([FromQuery] PageRequest pageRequest)
    {
        GetListTankQuery getListTankQuery = new() { PageRequest = pageRequest };
        GetListResponse<GetListTankListItemDto> response = await Mediator.Send(getListTankQuery);
        return Ok(response);
    }

    [HttpPost("by-dynamic")]
    public async Task<IActionResult> GetListByDynamic([FromBody] GetListByDynamicTankQuery query)
    {
        GetListResponse<GetListTankListItemDto> response = await Mediator.Send(query);
        return Ok(response);
    }
}