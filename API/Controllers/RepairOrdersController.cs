using Microsoft.AspNetCore.Mvc;
using MiNegocioCR.Api.Application.DTOs;
using MiNegocioCR.Api.Application.Interfaces.RepairOrders;
using MiNegocioCR.Api.Application.UseCases.RepairOrder;
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
    private readonly ISearchRepairOrdersUseCase _searchRepairOrdersUseCase;

    public RepairOrdersController(
    ICreateRepairOrderUseCase createRepairOrderUseCase,
    IUpdateRepairOrderStatusUseCase updateStatusUseCase,
    IGetRepairOrdersByBusinessUseCase getByBusinessUseCase,
    IGetRepairOrderByIdUseCase getByIdUseCase,
    IUpdateRepairOrderUseCase updateRepairOrderUseCase,
    IGetRepairOrderByBusinessIdAndStatusUseCase getByIdAndStatusUseCase,
    ISearchRepairOrdersUseCase searchRepairOrdersUseCase )
    {
        _createRepairOrderUseCase = createRepairOrderUseCase;
        _updateStatusUseCase = updateStatusUseCase;
        _getByBusinessUseCase = getByBusinessUseCase;
        _getByIdUseCase = getByIdUseCase;
        _updateRepairOrderUseCase = updateRepairOrderUseCase;
        _getByIdAndStatusUseCase = getByIdAndStatusUseCase;
        _searchRepairOrdersUseCase = searchRepairOrdersUseCase;
    }

    [HttpPost("{businessId}")]
    public async Task<IActionResult> Create(
        Guid businessId,
        [FromBody] CreateRepairOrderRequestDto request)
    {
        if (request == null) return BadRequest("RepairOrdersCreate - Request body is required.");

        var result = await _createRepairOrderUseCase.Execute(businessId, request);
        return Ok(result);
    }

    [HttpPatch("{businessId:guid}/{id:guid}/status")]
    public async Task<IActionResult> UpdateStatus(
    Guid businessId,
    Guid id,
    [FromBody] UpdateStatusRequestDto request)
    {
        if (request == null) return BadRequest("RepairOrdersUpdateStatus - Request body is required.");
        var result = await _updateStatusUseCase.Execute(businessId, id, request);
        return Ok(result);
    }

    [HttpGet("{businessId:guid}/search")]
    public async Task<IActionResult> Search(
        Guid businessId,
        [FromQuery] string? query = null,
        [FromQuery] DateTime? from = null,
        [FromQuery] DateTime? to = null)
    {
        var result = await _searchRepairOrdersUseCase.Execute(businessId, query, from, to);
        return Ok(result);
    }

    [HttpGet("business/{businessId}")]
    public async Task<IActionResult> GetByBusiness(Guid businessId)
    {
        var result = await _getByBusinessUseCase.Execute(businessId);
        return Ok(result);
    }

    [HttpGet("{businessId:guid}/{id:guid}")]
    public async Task<IActionResult> GetById(Guid businessId, Guid id)
    {
        var result = await _getByIdUseCase.Execute(businessId, id);

        if (result == null)
            return NotFound();

        return Ok(result);
    }

    [HttpPut("{businessId:guid}/{id:guid}")]
    public async Task<IActionResult> Update(
    Guid businessId,
    Guid id,
    [FromBody] UpdateRepairOrderRequestDto request)
    {
        if (request == null) return BadRequest("RepairOrdersUpdate - Request body is required.");
        var result = await _updateRepairOrderUseCase.Execute(businessId, id, request);
        return Ok(result);
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



