using Application.Features.Donors.Commands.Create;
using Application.Features.Donors.Commands.CreateRange;
using Application.Features.Donors.Commands.Delete;
using Application.Features.Donors.Commands.DeleteRange;
using Application.Features.Donors.Commands.Update;
using Application.Features.Donors.Commands.UpdateRange;
using Application.Features.Donors.Queries.GetById;
using Application.Features.Donors.Queries.GetList;
using Application.Features.Donors.Queries.GetListByDynamic;
using NArchitecture.Core.Application.Requests;
using NArchitecture.Core.Application.Responses;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class DonorsController : BaseController
{
    [HttpPost]
    public async Task<IActionResult> Add([FromBody] CreateDonorCommand createDonorCommand)
    {
        CreatedDonorResponse response = await Mediator.Send(createDonorCommand);

        return Created(uri: "", response);
    }

    [HttpPut]
    public async Task<IActionResult> Update([FromBody] UpdateDonorCommand updateDonorCommand)
    {
        UpdatedDonorResponse response = await Mediator.Send(updateDonorCommand);

        return Ok(response);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete([FromRoute] Guid id)
    {
        DeletedDonorResponse response = await Mediator.Send(new DeleteDonorCommand { Id = id });

        return Ok(response);
    }

    [HttpPost("range")]
    public async Task<IActionResult> AddRange([FromBody] CreateDonorRangeCommand createDonorRangeCommand)
    {
        CreatedDonorRangeResponse response = await Mediator.Send(createDonorRangeCommand);

        return Created(uri: "", response);
    }

    [HttpPut("range")]
    public async Task<IActionResult> UpdateRange([FromBody] UpdateDonorRangeCommand updateDonorRangeCommand)
    {
        UpdatedDonorRangeResponse response = await Mediator.Send(updateDonorRangeCommand);

        return Ok(response);
    }

    [HttpPost("delete-range")]
    public async Task<IActionResult> DeleteRange([FromBody] DeleteDonorRangeCommand deleteDonorRangeCommand)
    {
        DeletedDonorRangeResponse response = await Mediator.Send(deleteDonorRangeCommand);

        return Ok(response);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById([FromRoute] Guid id)
    {
        GetByIdDonorResponse response = await Mediator.Send(new GetByIdDonorQuery { Id = id });
        return Ok(response);
    }

    [HttpGet]
    public async Task<IActionResult> GetList([FromQuery] PageRequest pageRequest)
    {
        GetListDonorQuery getListDonorQuery = new() { PageRequest = pageRequest };
        GetListResponse<GetListDonorListItemDto> response = await Mediator.Send(getListDonorQuery);
        return Ok(response);
    }

    [HttpPost("by-dynamic")]
    public async Task<IActionResult> GetListByDynamic([FromBody] GetListByDynamicDonorQuery query)
    {
        GetListResponse<GetListDonorListItemDto> response = await Mediator.Send(query);
        return Ok(response);
    }
}