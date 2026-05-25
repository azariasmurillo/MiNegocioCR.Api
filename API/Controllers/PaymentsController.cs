using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiNegocioCR.Api.Application.DTOs;
using MiNegocioCR.Api.Application.Interfaces.UseCases.Payments;

namespace MiNegocioCR.Api.API.Controllers;

[Authorize]
[ApiController]
[Route("api/payments")]
public class PaymentsController : ControllerBase
{
    private readonly ICreatePaymentUseCase _createPaymentUseCase;
    private readonly IGetPaymentsByRepairOrderUseCase _getPaymentsByRepairOrderUseCase;

    public PaymentsController(
        ICreatePaymentUseCase createPaymentUseCase,
        IGetPaymentsByRepairOrderUseCase getPaymentsByRepairOrderUseCase)
    {
        _createPaymentUseCase = createPaymentUseCase;
        _getPaymentsByRepairOrderUseCase = getPaymentsByRepairOrderUseCase;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreatePaymentRequestDto request)
    {
        if (request == null) return BadRequest("Request body is required.");
        var result = await _createPaymentUseCase.Execute(request);
        return Ok(result);
    }

    /// <summary>Legacy/query style: businessId as query parameter.</summary>
    [HttpGet("repair/{repairOrderId:guid}")]
    public async Task<IActionResult> GetByRepairOrder(Guid repairOrderId, [FromQuery] Guid businessId)
    {
        if (businessId == Guid.Empty) return BadRequest("BusinessId is required.");
        var result = await _getPaymentsByRepairOrderUseCase.Execute(businessId, repairOrderId);
        return Ok(result);
    }

    /// <summary>GET /api/payments/business/{businessId}/repair-order/{repairOrderId}</summary>
    [HttpGet("business/{businessId:guid}/repair-order/{repairOrderId:guid}")]
    public Task<IActionResult> GetByBusinessAndRepairOrderPath(Guid businessId, Guid repairOrderId) =>
        ListPaymentsForRepairOrderAsync(businessId, repairOrderId);

    /// <summary>GET /api/payments/business/{businessId}?repairOrderId=...</summary>
    [HttpGet("business/{businessId:guid}")]
    public Task<IActionResult> GetByBusinessAndRepairOrderQuery(
        Guid businessId,
        [FromQuery] Guid repairOrderId)
    {
        if (repairOrderId == Guid.Empty)
            return Task.FromResult<IActionResult>(BadRequest("Query parameter repairOrderId is required."));
        return ListPaymentsForRepairOrderAsync(businessId, repairOrderId);
    }

    private async Task<IActionResult> ListPaymentsForRepairOrderAsync(Guid businessId, Guid repairOrderId)
    {
        if (businessId == Guid.Empty) return BadRequest("BusinessId is required.");
        var result = await _getPaymentsByRepairOrderUseCase.Execute(businessId, repairOrderId);
        return Ok(result);
    }
}
