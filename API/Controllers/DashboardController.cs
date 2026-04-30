using Microsoft.AspNetCore.Mvc;
using MiNegocioCR.Api.API.Http;
using MiNegocioCR.Api.Application.Interfaces.UseCases.Dashboard;

namespace MiNegocioCR.Api.API.Controllers;

[ApiController]
[Route("api/dashboard")]
public class DashboardController : ControllerBase
{
    private readonly IGetDashboardSummaryUseCase _getDashboardSummaryUseCase;
    private readonly IGetSalesTrendUseCase _getSalesTrendUseCase;
    private readonly IGetTicketAverageUseCase _getTicketAverageUseCase;
    private readonly IGetRecentActivityUseCase _getRecentActivityUseCase;

    public DashboardController(
        IGetDashboardSummaryUseCase getDashboardSummaryUseCase,
        IGetSalesTrendUseCase getSalesTrendUseCase,
        IGetTicketAverageUseCase getTicketAverageUseCase,
        IGetRecentActivityUseCase getRecentActivityUseCase)
    {
        _getDashboardSummaryUseCase = getDashboardSummaryUseCase;
        _getSalesTrendUseCase = getSalesTrendUseCase;
        _getTicketAverageUseCase = getTicketAverageUseCase;
        _getRecentActivityUseCase = getRecentActivityUseCase;
    }

    [HttpGet("{businessId:guid}/summary")]
    public async Task<IActionResult> Summary(Guid businessId, [FromQuery] string? from, [FromQuery] string? to)
    {
        if (businessId == Guid.Empty) return BadRequest("BusinessId is required.");
        var fromDt = QueryParamParsing.ParseUtcDayStart(from);
        var toDt = QueryParamParsing.ParseUtcDayStart(to);
        var result = await _getDashboardSummaryUseCase.Execute(businessId, fromDt, toDt);
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
        var fromDt = QueryParamParsing.ParseUtcDayStart(from);
        var toDt = QueryParamParsing.ParseUtcDayStart(to);
        var result = await _getSalesTrendUseCase.Execute(businessId, fromDt, toDt, groupBy);
        return Ok(result);
    }

    [HttpGet("{businessId:guid}/ticket-average")]
    public async Task<IActionResult> TicketAverage(Guid businessId, [FromQuery] string? from, [FromQuery] string? to)
    {
        if (businessId == Guid.Empty) return BadRequest("BusinessId is required.");
        var fromDt = QueryParamParsing.ParseUtcDayStart(from);
        var toDt = QueryParamParsing.ParseUtcDayStart(to);
        var result = await _getTicketAverageUseCase.Execute(businessId, fromDt, toDt);
        return Ok(result);
    }

    [HttpGet("{businessId:guid}/activity")]
    public async Task<IActionResult> Activity(Guid businessId, [FromQuery] string? limit = null)
    {
        if (businessId == Guid.Empty) return BadRequest("BusinessId is required.");
        var limitN = QueryParamParsing.ParsePositiveInt(limit, 20, 100);
        var result = await _getRecentActivityUseCase.Execute(businessId, limitN);
        return Ok(result);
    }
}
