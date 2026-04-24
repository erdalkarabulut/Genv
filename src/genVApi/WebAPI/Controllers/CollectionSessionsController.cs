using Application.Features.CollectionSessions.Commands.Calculate;
using Application.Features.CollectionSessions.Commands.Create;
using Application.Features.CollectionSessions.Commands.CreateRange;
using Application.Features.CollectionSessions.Commands.Delete;
using Application.Features.CollectionSessions.Commands.DeleteRange;
using Application.Features.CollectionSessions.Commands.Update;
using Application.Features.CollectionSessions.Commands.UpdateRange;
using Application.Features.CollectionSessions.Queries.GetById;
using Application.Features.CollectionSessions.Queries.GetList;
using Application.Features.CollectionSessions.Queries.GetListByDynamic;
using NArchitecture.Core.Application.Requests;
using NArchitecture.Core.Application.Responses;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class CollectionSessionsController : BaseController
{
    [HttpPost]
    public async Task<IActionResult> Add([FromBody] CreateCollectionSessionCommand createCollectionSessionCommand)
    {
        CreatedCollectionSessionResponse response = await Mediator.Send(createCollectionSessionCommand);

        return Created(uri: "", response);
    }

    [HttpPut]
    public async Task<IActionResult> Update([FromBody] UpdateCollectionSessionCommand updateCollectionSessionCommand)
    {
        UpdatedCollectionSessionResponse response = await Mediator.Send(updateCollectionSessionCommand);

        return Ok(response);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete([FromRoute] Guid id)
    {
        DeletedCollectionSessionResponse response = await Mediator.Send(new DeleteCollectionSessionCommand { Id = id });

        return Ok(response);
    }

    [HttpPost("range")]
    public async Task<IActionResult> AddRange([FromBody] CreateCollectionSessionRangeCommand createCollectionSessionRangeCommand)
    {
        CreatedCollectionSessionRangeResponse response = await Mediator.Send(createCollectionSessionRangeCommand);

        return Created(uri: "", response);
    }

    [HttpPut("range")]
    public async Task<IActionResult> UpdateRange([FromBody] UpdateCollectionSessionRangeCommand updateCollectionSessionRangeCommand)
    {
        UpdatedCollectionSessionRangeResponse response = await Mediator.Send(updateCollectionSessionRangeCommand);

        return Ok(response);
    }

    [HttpPost("delete-range")]
    public async Task<IActionResult> DeleteRange([FromBody] DeleteCollectionSessionRangeCommand deleteCollectionSessionRangeCommand)
    {
        DeletedCollectionSessionRangeResponse response = await Mediator.Send(deleteCollectionSessionRangeCommand);

        return Ok(response);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById([FromRoute] Guid id)
    {
        GetByIdCollectionSessionResponse response = await Mediator.Send(new GetByIdCollectionSessionQuery { Id = id });
        return Ok(response);
    }

    [HttpGet]
    public async Task<IActionResult> GetList([FromQuery] PageRequest pageRequest)
    {
        GetListCollectionSessionQuery getListCollectionSessionQuery = new() { PageRequest = pageRequest };
        GetListResponse<GetListCollectionSessionListItemDto> response = await Mediator.Send(getListCollectionSessionQuery);
        return Ok(response);
    }

    [HttpPost("by-dynamic")]
    public async Task<IActionResult> GetListByDynamic([FromBody] GetListByDynamicCollectionSessionQuery query)
    {
        GetListResponse<GetListCollectionSessionListItemDto> response = await Mediator.Send(query);
        return Ok(response);
    }

    [HttpPost("{id}/calculate")]
    public async Task<IActionResult> Calculate([FromRoute] Guid id)
    {
        CalculateSessionResponse response = await Mediator.Send(new CalculateSessionCommand { SessionId = id });
        return Ok(response);
    }
}