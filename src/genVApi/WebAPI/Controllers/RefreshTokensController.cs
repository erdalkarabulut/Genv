using Application.Features.RefreshTokens.Queries.GetList;
using Application.Features.RefreshTokens.Queries.GetListByDynamic;
using Microsoft.AspNetCore.Mvc;
using NArchitecture.Core.Application.Requests;
using NArchitecture.Core.Application.Responses;

namespace WebAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class RefreshTokensController : BaseController
{
    [HttpGet]
    public async Task<IActionResult> GetList([FromQuery] PageRequest pageRequest)
    {
        GetListRefreshTokenQuery getListRefreshTokenQuery = new() { PageRequest = pageRequest };
        GetListResponse<GetListRefreshTokenListItemDto> result = await Mediator.Send(getListRefreshTokenQuery);
        return Ok(result);
    }

    [HttpPost("by-dynamic")]
    public async Task<IActionResult> GetListByDynamic([FromBody] GetListByDynamicRefreshTokenQuery query)
    {
        GetListResponse<GetListRefreshTokenListItemDto> result = await Mediator.Send(query);
        return Ok(result);
    }
}
