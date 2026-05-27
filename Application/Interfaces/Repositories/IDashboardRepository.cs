using MiNegocioCR.Api.Application.DTOs;

namespace MiNegocioCR.Api.Application.Interfaces.Repositories;

public interface IDashboardRepository
{
    Task<DashboardSummaryDto> GetSummaryAsync(Guid businessId, DateTime? from, DateTime? to);
    Task<List<SalesTrendPointDto>> GetSalesTrendAsync(Guid businessId, DateTime? fromUtcInclusive, DateTime? toUtcExclusive, string groupBy);
    Task<TicketAverageDto> GetTicketAverageAsync(Guid businessId, DateTime? fromUtcInclusive, DateTime? toUtcExclusive);
    Task<List<ActivityItemDto>> GetRecentActivityAsync(Guid businessId, int limit);
    Task<List<TopProductRowDto>> GetTopProductsAsync(Guid businessId, int take);
    Task<List<PendingOrderRowDto>> GetPendingOrdersWithBalanceAsync(Guid businessId);
    Task<ProfitBySourceDto> GetProfitBySourceAsync(Guid businessId);
}
