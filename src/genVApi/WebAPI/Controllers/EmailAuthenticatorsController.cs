using Application.Features.EmailAuthenticators.Queries.GetList;
using Application.Features.EmailAuthenticators.Queries.GetListByDynamic;
using Microsoft.AspNetCore.Mvc;
using NArchitecture.Core.Application.Requests;
using NArchitecture.Core.Application.Responses;

namespace WebAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class EmailAuthenticatorsController : BaseController
{
    [HttpGet]
    public async Task<IActionResult> GetList([FromQuery] PageRequest pageRequest)
    {
        GetListEmailAuthenticatorQuery getListEmailAuthenticatorQuery = new() { PageRequest = pageRequest };
        GetListResponse<GetListEmailAuthenticatorListItemDto> result = await Mediator.Send(getListEmailAuthenticatorQuery);
        return Ok(result);
    }

    [HttpPost("by-dynamic")]
    public async Task<IActionResult> GetListByDynamic([FromBody] GetListByDynamicEmailAuthenticatorQuery query)
    {
        GetListResponse<GetListEmailAuthenticatorListItemDto> result = await Mediator.Send(query);
        return Ok(result);
    }
}
