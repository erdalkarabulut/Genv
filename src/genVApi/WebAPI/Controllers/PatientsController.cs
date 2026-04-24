using Application.Features.Patients.Commands.Create;
using Application.Features.Patients.Commands.CreateRange;
using Application.Features.Patients.Commands.Delete;
using Application.Features.Patients.Commands.DeleteRange;
using Application.Features.Patients.Commands.Update;
using Application.Features.Patients.Commands.UpdateRange;
using Application.Features.Patients.Queries.GetById;
using Application.Features.Patients.Queries.GetList;
using Application.Features.Patients.Queries.GetListByDynamic;
using NArchitecture.Core.Application.Requests;
using NArchitecture.Core.Application.Responses;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class PatientsController : BaseController
{
    [HttpPost]
    public async Task<IActionResult> Add([FromBody] CreatePatientCommand createPatientCommand)
    {
        CreatedPatientResponse response = await Mediator.Send(createPatientCommand);

        return Created(uri: "", response);
    }

    [HttpPut]
    public async Task<IActionResult> Update([FromBody] UpdatePatientCommand updatePatientCommand)
    {
        UpdatedPatientResponse response = await Mediator.Send(updatePatientCommand);

        return Ok(response);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete([FromRoute] Guid id)
    {
        DeletedPatientResponse response = await Mediator.Send(new DeletePatientCommand { Id = id });

        return Ok(response);
    }

    [HttpPost("range")]
    public async Task<IActionResult> AddRange([FromBody] CreatePatientRangeCommand createPatientRangeCommand)
    {
        CreatedPatientRangeResponse response = await Mediator.Send(createPatientRangeCommand);

        return Created(uri: "", response);
    }

    [HttpPut("range")]
    public async Task<IActionResult> UpdateRange([FromBody] UpdatePatientRangeCommand updatePatientRangeCommand)
    {
        UpdatedPatientRangeResponse response = await Mediator.Send(updatePatientRangeCommand);

        return Ok(response);
    }

    [HttpPost("delete-range")]
    public async Task<IActionResult> DeleteRange([FromBody] DeletePatientRangeCommand deletePatientRangeCommand)
    {
        DeletedPatientRangeResponse response = await Mediator.Send(deletePatientRangeCommand);

        return Ok(response);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById([FromRoute] Guid id)
    {
        GetByIdPatientResponse response = await Mediator.Send(new GetByIdPatientQuery { Id = id });
        return Ok(response);
    }

    [HttpGet]
    public async Task<IActionResult> GetList([FromQuery] PageRequest pageRequest)
    {
        GetListPatientQuery getListPatientQuery = new() { PageRequest = pageRequest };
        GetListResponse<GetListPatientListItemDto> response = await Mediator.Send(getListPatientQuery);
        return Ok(response);
    }

    [HttpPost("by-dynamic")]
    public async Task<IActionResult> GetListByDynamic([FromBody] GetListByDynamicPatientQuery query)
    {
        GetListResponse<GetListPatientListItemDto> response = await Mediator.Send(query);
        return Ok(response);
    }
}