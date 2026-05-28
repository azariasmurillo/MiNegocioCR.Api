using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiNegocioCR.Api.API.Http;
using MiNegocioCR.Api.Application.Interfaces.UseCases.Dashboard;

namespace MiNegocioCR.Api.API.Controllers;

[Authorize]
[ApiController]
[Route("api/dashboard")]
public class DashboardController : ControllerBase
{
    private readonly IGetDashboardSummaryUseCase _getDashboardSummaryUseCase;
    private readonly IGetSalesTrendUseCase _getSalesTrendUseCase;
    private readonly IGetTicketAverageUseCase _getTicketAverageUseCase;
    private readonly IGetRecentActivityUseCase _getRecentActivityUseCase;
    private readonly IGetTopProductsUseCase _getTopProductsUseCase;
    private readonly IGetPendingOrdersDashboardUseCase _getPendingOrdersDashboardUseCase;
    private readonly IGetProfitBySourceUseCase _getProfitBySourceUseCase;

    public DashboardController(
        IGetDashboardSummaryUseCase getDashboardSummaryUseCase,
        IGetSalesTrendUseCase getSalesTrendUseCase,
        IGetTicketAverageUseCase getTicketAverageUseCase,
        IGetRecentActivityUseCase getRecentActivityUseCase,
        IGetTopProductsUseCase getTopProductsUseCase,
        IGetPendingOrdersDashboardUseCase getPendingOrdersDashboardUseCase,
        IGetProfitBySourceUseCase getProfitBySourceUseCase)
    {
        _getDashboardSummaryUseCase = getDashboardSummaryUseCase;
        _getSalesTrendUseCase = getSalesTrendUseCase;
        _getTicketAverageUseCase = getTicketAverageUseCase;
        _getRecentActivityUseCase = getRecentActivityUseCase;
        _getTopProductsUseCase = getTopProductsUseCase;
        _getPendingOrdersDashboardUseCase = getPendingOrdersDashboardUseCase;
        _getProfitBySourceUseCase = getProfitBySourceUseCase;
    }

    [HttpGet("{businessId:guid}/summary")]
    public async Task<IActionResult> Summary(Guid businessId, [FromQuery] string? from, [FromQuery] string? to)
    {
        if (businessId == Guid.Empty) return BadRequest("BusinessId is required.");
        var (fromDt, toExclusive) = QueryParamParsing.ParseCostaRicaDateRange(from, to);
        var result = await _getDashboardSummaryUseCase.Execute(businessId, fromDt, toExclusive);
        return Ok(result);
    }

    [HttpGet("{businessId:guid}/sales-trend")]
    public async Task<IActionResult> SalesTrend(
        Guid businessId,
        [FromQuery] string? from,
        [FromQuery] string? to,
        [FromQuery] string? groupBy = "day")
    {
        if (businessId == Guid.Empty) return BadRequest("BusinessId is required.");
        var (fromDt, toExclusive) = QueryParamParsing.ParseCostaRicaDateRange(from, to);
        var result = await _getSalesTrendUseCase.Execute(businessId, fromDt, toExclusive, groupBy);
        return Ok(result);
    }

    [HttpGet("{businessId:guid}/ticket-average")]
    public async Task<IActionResult> TicketAverage(Guid businessId, [FromQuery] string? from, [FromQuery] string? to)
    {
        if (businessId == Guid.Empty) return BadRequest("BusinessId is required.");
        var (fromDt, toExclusive) = QueryParamParsing.ParseCostaRicaDateRange(from, to);
        var result = await _getTicketAverageUseCase.Execute(businessId, fromDt, toExclusive);
        return Ok(result);
    }

    [HttpGet("{businessId:guid}/activity")]
    public async Task<IActionResult> Activity(
        Guid businessId,
        [FromQuery] string? limit = null,
        [FromQuery] string? from = null,
        [FromQuery] string? to = null)
    {
        if (businessId == Guid.Empty) return BadRequest("BusinessId is required.");
        var limitN = QueryParamParsing.ParsePositiveInt(limit, 20, 100);
        var (fromDt, toExclusive) = QueryParamParsing.ParseCostaRicaDateRange(from, to);
        var result = await _getRecentActivityUseCase.Execute(businessId, limitN, fromDt, toExclusive);
        return Ok(result);
    }

    /// <summary>Top productos por ingresos (líneas tipo Product con variante de catálogo).</summary>
    [HttpGet("{businessId:guid}/top-products")]
    public async Task<IActionResult> TopProducts(
        Guid businessId,
        [FromQuery] int take = 10,
        [FromQuery] string? from = null,
        [FromQuery] string? to = null)
    {
        if (businessId == Guid.Empty) return BadRequest("BusinessId is required.");
        var (fromDt, toExclusive) = QueryParamParsing.ParseCostaRicaDateRange(from, to);
        var result = await _getTopProductsUseCase.Execute(businessId, take, fromDt, toExclusive);
        return Ok(result);
    }

    /// <summary>Órdenes de reparación con saldo pendiente (total orden − pagos).</summary>
    [HttpGet("{businessId:guid}/pending-orders")]
    public async Task<IActionResult> PendingOrders(Guid businessId)
    {
        if (businessId == Guid.Empty) return BadRequest("BusinessId is required.");
        var result = await _getPendingOrdersDashboardUseCase.Execute(businessId);
        return Ok(result);
    }

    /// <summary>Ganancia acumulada agrupada por <c>Sale.Source</c>.</summary>
    [HttpGet("{businessId:guid}/profit-by-source")]
    public async Task<IActionResult> ProfitBySource(
        Guid businessId,
        [FromQuery] string? from = null,
        [FromQuery] string? to = null)
    {
        if (businessId == Guid.Empty) return BadRequest("BusinessId is required.");
        var (fromDt, toExclusive) = QueryParamParsing.ParseCostaRicaDateRange(from, to);
        var result = await _getProfitBySourceUseCase.Execute(businessId, fromDt, toExclusive);
        return Ok(result);
    }
}
