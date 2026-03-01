using Microsoft.AspNetCore.Mvc;
using MiNegocioCR.Api.Aplication.DTOs;
using MiNegocioCR.Api.Aplication.Interfaces.ReapirOrders;
using MiNegocioCR.Api.Aplication.UseCases.RepairOrder;
using MiNegocioCR.Api.Domain.Enums;

namespace MiNegocioCR.Api.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RepairOrdersController : ControllerBase
{
    private readonly ICreateRepairOrderUseCase _createRepairOrderUseCase;
    private readonly IUpdateRepairOrderStatusUseCase _updateStatusUseCase;
    private readonly IGetRepairOrdersByBusinessUseCase _getByBusinessUseCase;
    private readonly IGetRepairOrderByIdUseCase _getByIdUseCase;
    private readonly IUpdateRepairOrderUseCase _updateRepairOrderUseCase;
    private readonly IGetRepairOrderByBusinessIdAndStatusUseCase _getByIdAndStatusUseCase;

    public RepairOrdersController(
    ICreateRepairOrderUseCase createRepairOrderUseCase,
    IUpdateRepairOrderStatusUseCase updateStatusUseCase,
    IGetRepairOrdersByBusinessUseCase getByBusinessUseCase,
    IGetRepairOrderByIdUseCase getByIdUseCase,
    IUpdateRepairOrderUseCase updateRepairOrderUseCase,
    IGetRepairOrderByBusinessIdAndStatusUseCase getByIdAndStatusUseCase )
    {
        _createRepairOrderUseCase = createRepairOrderUseCase;
        _updateStatusUseCase = updateStatusUseCase;
        _getByBusinessUseCase = getByBusinessUseCase;
        _getByIdUseCase = getByIdUseCase;
        _updateRepairOrderUseCase = updateRepairOrderUseCase;
        _getByIdAndStatusUseCase = getByIdAndStatusUseCase;
    }

    [HttpPost("{businessId}")]
    public async Task<IActionResult> Create(
        Guid businessId,
        [FromBody] CreateRepairOrderRequestDto request)
    {
        var result = await _createRepairOrderUseCase.Execute(businessId, request);
        return Ok(result);
    }

    [HttpPatch("{id}/status")]
    public async Task<IActionResult> UpdateStatus(
    Guid id,
    [FromBody] UpdateStatusRequestDto request)
    {
        var result = await _updateStatusUseCase.Execute(id, request);
        return Ok(result);
    }

    [HttpGet("business/{businessId}")]
    public async Task<IActionResult> GetByBusiness(Guid businessId)
    {
        var result = await _getByBusinessUseCase.Execute(businessId);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _getByIdUseCase.Execute(id);

        if (result == null)
            return NotFound();

        return Ok(result);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(
    Guid id,
    [FromBody] UpdateRepairOrderRequestDto request)
    {
        await _updateRepairOrderUseCase.Execute(id, request);
        return Ok();
    }

    [HttpGet("{businessId}/by-status")]  // o "{businessId}/status"
    public async Task<IActionResult> GetByBusinessIdAndStatus(
    Guid businessId,
    [FromQuery] RepairOrderStatus status)
    {
        var result = await _getByIdAndStatusUseCase.Execute(businessId, status);
        return Ok(result);  
    }

}



