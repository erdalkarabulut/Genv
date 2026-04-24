using Application.Features.Dashboard.Queries;
using Application.Features.Slots.Queries.GetCryoGrid;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class DashboardController : BaseController
{
    [HttpGet]
    public async Task<IActionResult> Get()
    {
        DashboardResponse response = await Mediator.Send(new GetDashboardQuery());
        return Ok(response);
    }

    [HttpGet("cryo-grid")]
    public async Task<IActionResult> GetCryoGrid([FromQuery] Guid? tankId)
    {
        CryoGridResponse response = await Mediator.Send(new GetCryoGridQuery { TankId = tankId });
        return Ok(response);
    }
}
