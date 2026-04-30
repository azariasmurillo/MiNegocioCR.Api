using MiNegocioCR.Api.Application.DTOs;

namespace MiNegocioCR.Api.Application.Interfaces.Repositories;

public interface IDashboardRepository
{
    Task<DashboardSummaryDto> GetSummaryAsync(Guid businessId, DateTime? from, DateTime? to);
    Task<List<SalesTrendPointDto>> GetSalesTrendAsync(Guid businessId, DateTime? from, DateTime? to, string groupBy);
    Task<TicketAverageDto> GetTicketAverageAsync(Guid businessId, DateTime? from, DateTime? to);
    Task<List<ActivityItemDto>> GetRecentActivityAsync(Guid businessId, int limit);
}
