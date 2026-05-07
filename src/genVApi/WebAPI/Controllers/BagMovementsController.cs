using Application.Features.BagMovements.Commands.Create;
using Application.Features.BagMovements.Commands.CreateRange;
using Application.Features.BagMovements.Commands.Delete;
using Application.Features.BagMovements.Commands.DeleteRange;
using Application.Features.BagMovements.Commands.Update;
using Application.Features.BagMovements.Commands.UpdateRange;
using Application.Features.BagMovements.Queries.GetById;
using Application.Features.BagMovements.Queries.GetList;
using Application.Features.BagMovements.Queries.GetListByDynamic;
using Application.Features.BagMovements.Queries.Search;
using NArchitecture.Core.Application.Requests;
using NArchitecture.Core.Application.Responses;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class BagMovementsController : BaseController
{
    [HttpPost]
    public async Task<IActionResult> Add([FromBody] CreateBagMovementCommand createBagMovementCommand)
    {
        CreatedBagMovementResponse response = await Mediator.Send(createBagMovementCommand);

        return Created(uri: "", response);
    }

    [HttpPut]
    public async Task<IActionResult> Update([FromBody] UpdateBagMovementCommand updateBagMovementCommand)
    {
        UpdatedBagMovementResponse response = await Mediator.Send(updateBagMovementCommand);

        return Ok(response);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete([FromRoute] Guid id)
    {
        DeletedBagMovementResponse response = await Mediator.Send(new DeleteBagMovementCommand { Id = id });

        return Ok(response);
    }

    [HttpPost("range")]
    public async Task<IActionResult> AddRange([FromBody] CreateBagMovementRangeCommand createBagMovementRangeCommand)
    {
        CreatedBagMovementRangeResponse response = await Mediator.Send(createBagMovementRangeCommand);

        return Created(uri: "", response);
    }

    [HttpPut("range")]
    public async Task<IActionResult> UpdateRange([FromBody] UpdateBagMovementRangeCommand updateBagMovementRangeCommand)
    {
        UpdatedBagMovementRangeResponse response = await Mediator.Send(updateBagMovementRangeCommand);

        return Ok(response);
    }

    [HttpPost("delete-range")]
    public async Task<IActionResult> DeleteRange([FromBody] DeleteBagMovementRangeCommand deleteBagMovementRangeCommand)
    {
        DeletedBagMovementRangeResponse response = await Mediator.Send(deleteBagMovementRangeCommand);

        return Ok(response);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById([FromRoute] Guid id)
    {
        GetByIdBagMovementResponse response = await Mediator.Send(new GetByIdBagMovementQuery { Id = id });
        return Ok(response);
    }

    [HttpGet]
    public async Task<IActionResult> GetList([FromQuery] PageRequest pageRequest)
    {
        GetListBagMovementQuery getListBagMovementQuery = new() { PageRequest = pageRequest };
        GetListResponse<GetListBagMovementListItemDto> response = await Mediator.Send(getListBagMovementQuery);
        return Ok(response);
    }

    [HttpPost("search")]
    public async Task<IActionResult> Search([FromBody] SearchBagMovementQuery query)
    {
        GetListResponse<GetListBagMovementListItemDto> response = await Mediator.Send(query);
        return Ok(response);
    }

    [HttpPost("by-dynamic")]
    public async Task<IActionResult> GetListByDynamic([FromBody] GetListByDynamicBagMovementQuery query)
    {
        GetListResponse<GetListBagMovementListItemDto> response = await Mediator.Send(query);
        return Ok(response);
    }
}