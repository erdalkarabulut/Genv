using Application.Features.ApheresisPlans.Queries.GetPlan;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ApheresisPlansController : BaseController
{
    /// <summary>
    /// Hastanın transplant tipine göre aferez planını (maksimum gün, hedef CD34,
    /// tamamlanan günler, kümülatif CD34/CD3, sonraki gün önerisi) döner.
    /// </summary>
    [HttpGet("{patientId}")]
    public async Task<IActionResult> GetPlan([FromRoute] Guid patientId)
    {
        ApheresisPlanResponse response = await Mediator.Send(new GetApheresisPlanQuery { PatientId = patientId });
        return Ok(response);
    }
}
