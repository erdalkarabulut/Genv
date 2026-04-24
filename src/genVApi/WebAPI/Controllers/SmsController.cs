using Application.Features.Sms.Commands.SendTestSms;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers;

/// <summary>SMS entegrasyon testi ve ileride olay bildirimleri için.</summary>
[Route("api/[controller]")]
[ApiController]
public class SmsController : BaseController
{
    /// <summary>Yalnızca Admin — sağlayıcıdan test SMS gönderir.</summary>
    [HttpPost("test")]
    public async Task<IActionResult> SendTest([FromBody] SendTestSmsCommand command)
    {
        SendTestSmsResponse result = await Mediator.Send(command);
        return Ok(result);
    }
}
