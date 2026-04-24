using Application.Features.ClinicalConfiguration;
using Application.Features.ClinicalConfiguration.Commands.UpdateClinicalSettings;
using Application.Features.ClinicalConfiguration.Queries.GetClinicalSettings;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers;

/// <summary>
/// Klinik CD34/CD3 hesap bölenleri ve kabul eşikleri — okuma çoğu kullanıcıya, güncelleme yalnız Admin.
/// </summary>
[Route("api/[controller]")]
[ApiController]
public class ClinicalSettingsController : BaseController
{
    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
    {
        ClinicalSettingsDto result = await Mediator.Send(new GetClinicalSettingsQuery(), cancellationToken);
        return Ok(result);
    }

    [HttpPut]
    public async Task<IActionResult> Put([FromBody] UpdateClinicalSettingsCommand command, CancellationToken cancellationToken)
    {
        ClinicalSettingsDto result = await Mediator.Send(command, cancellationToken);
        return Ok(result);
    }
}
