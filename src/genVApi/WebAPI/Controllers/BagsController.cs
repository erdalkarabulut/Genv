using Application.Features.Bags.Commands.Create;
using Application.Features.Bags.Commands.CreateRange;
using Application.Features.Bags.Commands.CustomSplit;
using Application.Features.Bags.Commands.Delete;
using Application.Features.Bags.Commands.DeleteRange;
using Application.Features.Bags.Commands.Freeze;
using Application.Features.Bags.Commands.Move;
using Application.Features.Bags.Commands.Split;
using Application.Features.Bags.Commands.Store;
using Application.Features.Bags.Commands.Update;
using Application.Features.Bags.Commands.UpdateRange;
using Application.Features.Bags.Commands.Use;
using Application.Features.Bags.Queries.GetById;
using Application.Features.Bags.Queries.GetList;
using Application.Features.Bags.Queries.GetListByDynamic;
using NArchitecture.Core.Application.Requests;
using NArchitecture.Core.Application.Responses;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class BagsController : BaseController
{
    [HttpPost]
    public async Task<IActionResult> Add([FromBody] CreateBagCommand createBagCommand)
    {
        CreatedBagResponse response = await Mediator.Send(createBagCommand);

        return Created(uri: "", response);
    }

    [HttpPut]
    public async Task<IActionResult> Update([FromBody] UpdateBagCommand updateBagCommand)
    {
        UpdatedBagResponse response = await Mediator.Send(updateBagCommand);

        return Ok(response);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete([FromRoute] Guid id)
    {
        DeletedBagResponse response = await Mediator.Send(new DeleteBagCommand { Id = id });

        return Ok(response);
    }

    [HttpPost("range")]
    public async Task<IActionResult> AddRange([FromBody] CreateBagRangeCommand createBagRangeCommand)
    {
        CreatedBagRangeResponse response = await Mediator.Send(createBagRangeCommand);

        return Created(uri: "", response);
    }

    [HttpPut("range")]
    public async Task<IActionResult> UpdateRange([FromBody] UpdateBagRangeCommand updateBagRangeCommand)
    {
        UpdatedBagRangeResponse response = await Mediator.Send(updateBagRangeCommand);

        return Ok(response);
    }

    [HttpPost("delete-range")]
    public async Task<IActionResult> DeleteRange([FromBody] DeleteBagRangeCommand deleteBagRangeCommand)
    {
        DeletedBagRangeResponse response = await Mediator.Send(deleteBagRangeCommand);

        return Ok(response);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById([FromRoute] Guid id)
    {
        GetByIdBagResponse response = await Mediator.Send(new GetByIdBagQuery { Id = id });
        return Ok(response);
    }

    [HttpGet]
    public async Task<IActionResult> GetList([FromQuery] PageRequest pageRequest)
    {
        GetListBagQuery getListBagQuery = new() { PageRequest = pageRequest };
        GetListResponse<GetListBagListItemDto> response = await Mediator.Send(getListBagQuery);
        return Ok(response);
    }

    [HttpPost("by-dynamic")]
    public async Task<IActionResult> GetListByDynamic([FromBody] GetListByDynamicBagQuery query)
    {
        GetListResponse<GetListBagListItemDto> response = await Mediator.Send(query);
        return Ok(response);
    }

    [HttpPost("store")]
    public async Task<IActionResult> Store([FromBody] StoreBagCommand command)
    {
        StoreBagResponse response = await Mediator.Send(command);
        return Ok(response);
    }

    [HttpPost("move")]
    public async Task<IActionResult> Move([FromBody] MoveBagCommand command)
    {
        MoveBagResponse response = await Mediator.Send(command);
        return Ok(response);
    }

    [HttpPost("use")]
    public async Task<IActionResult> Use([FromBody] UseBagCommand command)
    {
        UseBagResponse response = await Mediator.Send(command);
        return Ok(response);
    }

    /// <summary>
    /// Aferez ürününü N torbaya böler (varsayılan 4) ve 1 tanesini Cryo amacıyla işaretler.
    /// CryoSlotId verilirse Cryo torbasını doğrudan o slota store eder.
    /// </summary>
    [HttpPost("split")]
    public async Task<IActionResult> Split([FromBody] SplitSessionIntoBagsCommand command)
    {
        SplitSessionIntoBagsResponse response = await Mediator.Send(command);
        return Ok(response);
    }

    /// <summary>
    /// Aferez ürününü kullanıcının belirlediği hacim/WBC/yüzde değerleri ile birden çok torbaya böler.
    /// Her torba için isteğe bağlı olarak doğrudan dondurma slotu atanabilir.
    /// </summary>
    [HttpPost("custom-split")]
    public async Task<IActionResult> CustomSplit([FromBody] CustomSplitSessionIntoBagsCommand command)
    {
        CustomSplitSessionIntoBagsResponse response = await Mediator.Send(command);
        return Ok(response);
    }

    /// <summary>
    /// Mevcut torbayı dondurulmuş olarak işaretler. Torba henüz slotta değilse SlotId zorunludur.
    /// </summary>
    [HttpPost("freeze")]
    public async Task<IActionResult> Freeze([FromBody] FreezeBagCommand command)
    {
        FreezeBagResponse response = await Mediator.Send(command);
        return Ok(response);
    }
}