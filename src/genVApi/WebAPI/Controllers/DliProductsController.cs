using Application.Features.DliProducts.Commands.Create;
using Application.Features.DliProducts.Commands.CreateRange;
using Application.Features.DliProducts.Commands.Delete;
using Application.Features.DliProducts.Commands.DeleteRange;
using Application.Features.DliProducts.Commands.Update;
using Application.Features.DliProducts.Commands.UpdateRange;
using Application.Features.DliProducts.Queries.GetById;
using Application.Features.DliProducts.Queries.GetList;
using Application.Features.DliProducts.Queries.GetListByDynamic;
using NArchitecture.Core.Application.Requests;
using NArchitecture.Core.Application.Responses;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class DliProductsController : BaseController
{
    [HttpPost]
    public async Task<IActionResult> Add([FromBody] CreateDliProductCommand createDliProductCommand)
    {
        CreatedDliProductResponse response = await Mediator.Send(createDliProductCommand);

        return Created(uri: "", response);
    }

    [HttpPut]
    public async Task<IActionResult> Update([FromBody] UpdateDliProductCommand updateDliProductCommand)
    {
        UpdatedDliProductResponse response = await Mediator.Send(updateDliProductCommand);

        return Ok(response);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete([FromRoute] Guid id)
    {
        DeletedDliProductResponse response = await Mediator.Send(new DeleteDliProductCommand { Id = id });

        return Ok(response);
    }

    [HttpPost("range")]
    public async Task<IActionResult> AddRange([FromBody] CreateDliProductRangeCommand createDliProductRangeCommand)
    {
        CreatedDliProductRangeResponse response = await Mediator.Send(createDliProductRangeCommand);

        return Created(uri: "", response);
    }

    [HttpPut("range")]
    public async Task<IActionResult> UpdateRange([FromBody] UpdateDliProductRangeCommand updateDliProductRangeCommand)
    {
        UpdatedDliProductRangeResponse response = await Mediator.Send(updateDliProductRangeCommand);

        return Ok(response);
    }

    [HttpPost("delete-range")]
    public async Task<IActionResult> DeleteRange([FromBody] DeleteDliProductRangeCommand deleteDliProductRangeCommand)
    {
        DeletedDliProductRangeResponse response = await Mediator.Send(deleteDliProductRangeCommand);

        return Ok(response);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById([FromRoute] Guid id)
    {
        GetByIdDliProductResponse response = await Mediator.Send(new GetByIdDliProductQuery { Id = id });
        return Ok(response);
    }

    [HttpGet]
    public async Task<IActionResult> GetList([FromQuery] PageRequest pageRequest)
    {
        GetListDliProductQuery getListDliProductQuery = new() { PageRequest = pageRequest };
        GetListResponse<GetListDliProductListItemDto> response = await Mediator.Send(getListDliProductQuery);
        return Ok(response);
    }

    [HttpPost("by-dynamic")]
    public async Task<IActionResult> GetListByDynamic([FromBody] GetListByDynamicDliProductQuery query)
    {
        GetListResponse<GetListDliProductListItemDto> response = await Mediator.Send(query);
        return Ok(response);
    }
}