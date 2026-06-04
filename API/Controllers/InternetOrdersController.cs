using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiNegocioCR.Api.Application.DTOs;
using MiNegocioCR.Api.Application.Interfaces.InternetOrders;

namespace MiNegocioCR.Api.API.Controllers;

[Authorize]
[ApiController]
[Route("api/internet-orders")]
public class InternetOrdersController : ControllerBase
{
    private readonly ICreateInternetOrderUseCase _create;
    private readonly IUpdateInternetOrderUseCase _update;
    private readonly IGetInternetOrderByIdUseCase _getById;
    private readonly IListInternetOrdersByBusinessUseCase _list;
    private readonly IUpdateInternetOrderStatusUseCase _updateStatus;
    private readonly ISendInternetOrderEmailUseCase _sendEmail;

    public InternetOrdersController(
        ICreateInternetOrderUseCase create,
        IUpdateInternetOrderUseCase update,
        IGetInternetOrderByIdUseCase getById,
        IListInternetOrdersByBusinessUseCase list,
        IUpdateInternetOrderStatusUseCase updateStatus,
        ISendInternetOrderEmailUseCase sendEmail)
    {
        _create = create;
        _update = update;
        _getById = getById;
        _list = list;
        _updateStatus = updateStatus;
        _sendEmail = sendEmail;
    }

    [HttpGet("business/{businessId:guid}")]
    public async Task<IActionResult> List(
        Guid businessId,
        [FromQuery] string? status = null,
        [FromQuery] string? search = null)
    {
        var result = await _list.Execute(businessId, status, search);
        return Ok(result);
    }

    [HttpGet("{businessId:guid}/{id:guid}")]
    public async Task<IActionResult> GetById(Guid businessId, Guid id)
    {
        var result = await _getById.Execute(businessId, id);
        return result == null ? NotFound() : Ok(result);
    }

    [HttpPost("{businessId:guid}")]
    public async Task<IActionResult> Create(Guid businessId, [FromBody] UpsertInternetOrderRequestDto request)
    {
        if (request == null) return BadRequest("Request body is required.");
        var result = await _create.Execute(businessId, request);
        return Ok(result);
    }

    [HttpPut("{businessId:guid}/{id:guid}")]
    public async Task<IActionResult> Update(
        Guid businessId,
        Guid id,
        [FromBody] UpsertInternetOrderRequestDto request)
    {
        if (request == null) return BadRequest("Request body is required.");
        var result = await _update.Execute(businessId, id, request);
        return Ok(result);
    }

    [HttpPatch("{businessId:guid}/{id:guid}/status")]
    public async Task<IActionResult> UpdateStatus(
        Guid businessId,
        Guid id,
        [FromBody] UpdateInternetOrderStatusRequestDto request)
    {
        if (request == null) return BadRequest("Request body is required.");
        var result = await _updateStatus.Execute(businessId, id, request);
        return Ok(result);
    }

    [HttpPost("{businessId:guid}/{id:guid}/send-email")]
    public async Task<IActionResult> SendEmail(
        Guid businessId,
        Guid id,
        [FromBody] SendEmailRequestDto request)
    {
        if (request == null || string.IsNullOrWhiteSpace(request.HtmlContent))
            return BadRequest("htmlContent is required.");
        await _sendEmail.Execute(businessId, id, request.HtmlContent, request.Email);
        return Ok(new { message = "Email enviado correctamente" });
    }
}
