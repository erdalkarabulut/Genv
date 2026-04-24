using Application.Features.OtpAuthenticators.Queries.GetList;
using Application.Features.OtpAuthenticators.Queries.GetListByDynamic;
using Microsoft.AspNetCore.Mvc;
using NArchitecture.Core.Application.Requests;
using NArchitecture.Core.Application.Responses;

namespace WebAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class OtpAuthenticatorsController : BaseController
{
    [HttpGet]
    public async Task<IActionResult> GetList([FromQuery] PageRequest pageRequest)
    {
        GetListOtpAuthenticatorQuery getListOtpAuthenticatorQuery = new() { PageRequest = pageRequest };
        GetListResponse<GetListOtpAuthenticatorListItemDto> result = await Mediator.Send(getListOtpAuthenticatorQuery);
        return Ok(result);
    }

    [HttpPost("by-dynamic")]
    public async Task<IActionResult> GetListByDynamic([FromBody] GetListByDynamicOtpAuthenticatorQuery query)
    {
        GetListResponse<GetListOtpAuthenticatorListItemDto> result = await Mediator.Send(query);
        return Ok(result);
    }
}
