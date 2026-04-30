using Microsoft.EntityFrameworkCore;
using MiNegocioCR.Api.Application.DTOs;
using MiNegocioCR.Api.Application.Interfaces.Repositories;
using MiNegocioCR.Api.Domain.Enums;

namespace MiNegocioCR.Api.Infrastructure.Persistence.Repositories;

public class DashboardRepository : IDashboardRepository
{
    private readonly AppDbContext _context;

    public DashboardRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<DashboardSummaryDto> GetSummaryAsync(Guid businessId, DateTime? from, DateTime? to)
    {
        var startToday = DateTime.UtcNow.Date;
        var endToday = startToday.AddDays(1);

        var salesTodayQuery = _context.Sales
            .AsNoTracking()
            .Where(x => x.BusinessId == businessId && x.CreatedAt >= startToday && x.CreatedAt < endToday);

        var salesTodayCount = await salesTodayQuery.CountAsync();
        var salesTodayTotal = await salesTodayQuery
            .Select(x => (decimal?)(x.Total > 0 ? x.Total : x.TotalAmount))
            .SumAsync() ?? 0m;

        var ordersQuery = _context.RepairOrders
            .AsNoTracking()
            .Where(x => x.BusinessId == businessId);

        if (from.HasValue)
        {
            ordersQuery = ordersQuery.Where(x => x.CreatedAt >= from.Value.Date);
        }

        if (to.HasValue)
        {
            ordersQuery = ordersQuery.Where(x => x.CreatedAt < to.Value.Date.AddDays(1));
        }

        var groupedStatuses = await ordersQuery
            .GroupBy(x => x.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToListAsync();

        var invoicesTodayCount = salesTodayCount;

        return new DashboardSummaryDto
        {
            SalesTodayCount = salesTodayCount,
            SalesTodayTotal = salesTodayTotal,
            OrdersPendingCount = groupedStatuses.FirstOrDefault(x => x.Status == (int)RepairOrderStatus.Pending)?.Count ?? 0,
            OrdersInProcessCount = groupedStatuses.FirstOrDefault(x => x.Status == (int)RepairOrderStatus.InProcess)?.Count ?? 0,
            OrdersProcessedCount = groupedStatuses.FirstOrDefault(x => x.Status == (int)RepairOrderStatus.Processed)?.Count ?? 0,
            OrdersDeliveredCount = groupedStatuses.FirstOrDefault(x => x.Status == (int)RepairOrderStatus.Delivered)?.Count ?? 0,
            InvoicesTodayCount = invoicesTodayCount
        };
    }

    public async Task<List<SalesTrendPointDto>> GetSalesTrendAsync(Guid businessId, DateTime? from, DateTime? to, string groupBy)
    {
        var salesQuery = _context.Sales
            .AsNoTracking()
            .Where(x => x.BusinessId == businessId);

        if (from.HasValue)
        {
            salesQuery = salesQuery.Where(x => x.CreatedAt >= from.Value.Date);
        }

        if (to.HasValue)
        {
            salesQuery = salesQuery.Where(x => x.CreatedAt < to.Value.Date.AddDays(1));
        }

        if (groupBy == "month")
        {
            return await salesQuery
                .GroupBy(x => new { x.CreatedAt.Year, x.CreatedAt.Month })
                .OrderBy(x => x.Key.Year)
                .ThenBy(x => x.Key.Month)
                .Select(x => new SalesTrendPointDto
                {
                    Date = $"{x.Key.Year:D4}-{x.Key.Month:D2}",
                    Total = x.Sum(s => s.Total > 0 ? s.Total : s.TotalAmount)
                })
                .ToListAsync();
        }

        return await salesQuery
            .GroupBy(x => x.CreatedAt.Date)
            .OrderBy(x => x.Key)
            .Select(x => new SalesTrendPointDto
            {
                Date = x.Key.ToString("yyyy-MM-dd"),
                Total = x.Sum(s => s.Total > 0 ? s.Total : s.TotalAmount)
            })
            .ToListAsync();
    }

    public async Task<TicketAverageDto> GetTicketAverageAsync(Guid businessId, DateTime? from, DateTime? to)
    {
        var salesQuery = _context.Sales
            .AsNoTracking()
            .Where(x => x.BusinessId == businessId);

        if (from.HasValue)
        {
            salesQuery = salesQuery.Where(x => x.CreatedAt >= from.Value.Date);
        }

        if (to.HasValue)
        {
            salesQuery = salesQuery.Where(x => x.CreatedAt < to.Value.Date.AddDays(1));
        }

        var average = await salesQuery
            .Select(x => (decimal?)(x.Total > 0 ? x.Total : x.TotalAmount))
            .AverageAsync() ?? 0m;

        return new TicketAverageDto
        {
            AverageTicket = average
        };
    }

    public async Task<List<ActivityItemDto>> GetRecentActivityAsync(Guid businessId, int limit)
    {
        // EF Core cannot translate Union across projected DTOs to SQL. Load capped slices per source, merge in memory.
        var perSource = Math.Min(Math.Max(limit * 4, 40), 300);

        var saleActivities = await _context.Sales
            .AsNoTracking()
            .Where(x => x.BusinessId == businessId)
            .OrderByDescending(x => x.CreatedAt)
            .Take(perSource)
            .Select(x => new ActivityItemDto
            {
                Type = "sale_created",
                Description = $"Venta {x.InvoiceNumber} creada",
                CreatedAt = x.CreatedAt
            })
            .ToListAsync();

        var orderActivities = await _context.RepairOrders
            .AsNoTracking()
            .Where(x => x.BusinessId == businessId)
            .OrderByDescending(x => x.UpdatedAt)
            .Take(perSource)
            .Select(x => new ActivityItemDto
            {
                Type = "order_updated",
                Description = $"Orden {x.OrderNumber} actualizada (estado {x.Status})",
                CreatedAt = x.UpdatedAt
            })
            .ToListAsync();

        var invoiceActivities = await _context.Sales
            .AsNoTracking()
            .Where(x => x.BusinessId == businessId)
            .OrderByDescending(x => x.CreatedAt)
            .Take(perSource)
            .Select(x => new ActivityItemDto
            {
                Type = "invoice_sent",
                Description = $"Factura {x.InvoiceNumber} enviada",
                CreatedAt = x.CreatedAt
            })
            .ToListAsync();

        return saleActivities
            .Concat(orderActivities)
            .Concat(invoiceActivities)
            .OrderByDescending(x => x.CreatedAt)
            .Take(limit)
            .ToList();
    }
}
