using Microsoft.EntityFrameworkCore;
using MiNegocioCR.Api.Application.Common;
using MiNegocioCR.Api.Application.DTOs;
using MiNegocioCR.Api.Application.Interfaces.Repositories;
using MiNegocioCR.Api.Domain.Entities;
using MiNegocioCR.Api.Domain.Enums;

namespace MiNegocioCR.Api.Infrastructure.Persistence.Repositories;

public class DashboardRepository : IDashboardRepository
{
    private readonly AppDbContext _context;

    public DashboardRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<DashboardSummaryDto> GetSummaryAsync(Guid businessId, DateTime? from, DateTime? toExclusive)
    {
        var salesQuery = _context.Sales
            .AsNoTracking()
            .Where(x => x.BusinessId == businessId);

        if (from.HasValue)
            salesQuery = salesQuery.Where(x => x.CreatedAt >= from.Value);
        else if (!toExclusive.HasValue)
        {
            var startToday = CostaRicaTime.ToUtcStartOfDay(CostaRicaTime.Today);
            var endToday = CostaRicaTime.ToUtcEndExclusive(CostaRicaTime.Today);
            salesQuery = salesQuery.Where(x => x.CreatedAt >= startToday && x.CreatedAt < endToday);
        }

        if (toExclusive.HasValue)
            salesQuery = salesQuery.Where(x => x.CreatedAt < toExclusive.Value);

        var salesCount = await salesQuery.CountAsync();
        var ingresosHoy = await salesQuery
            .Select(x => (decimal?)(x.Total > 0 ? x.Total : x.TotalAmount))
            .SumAsync() ?? 0m;

        var gananciaHoy = await salesQuery
            .Select(x => (decimal?)x.TotalProfit)
            .SumAsync() ?? 0m;

        var ticketPromedio = salesCount > 0
            ? Math.Round(ingresosHoy / salesCount, 2, MidpointRounding.AwayFromZero)
            : 0m;

        var ordenesActivas = await _context.RepairOrders
            .AsNoTracking()
            .CountAsync(o => o.BusinessId == businessId
                && o.IsActive
                && (o.Status == (int)RepairOrderStatus.Pending
                    || o.Status == (int)RepairOrderStatus.InProcess
                    || o.Status == (int)RepairOrderStatus.Processed));

        return new DashboardSummaryDto
        {
            IngresosHoy = ingresosHoy,
            GananciaHoy = gananciaHoy,
            TicketPromedio = ticketPromedio,
            OrdenesActivas = ordenesActivas
        };
    }

    public async Task<List<SalesTrendPointDto>> GetSalesTrendAsync(
        Guid businessId,
        DateTime? from,
        DateTime? toExclusive,
        string groupBy)
    {
        var salesQuery = _context.Sales
            .AsNoTracking()
            .Where(x => x.BusinessId == businessId);

        if (from.HasValue)
            salesQuery = salesQuery.Where(x => x.CreatedAt >= from.Value);

        if (toExclusive.HasValue)
            salesQuery = salesQuery.Where(x => x.CreatedAt < toExclusive.Value);

        var rows = await salesQuery
            .Select(s => new
            {
                s.CreatedAt,
                Ingresos = s.Total > 0 ? s.Total : s.TotalAmount,
                s.TotalProfit
            })
            .ToListAsync();

        if (string.Equals(groupBy, "month", StringComparison.OrdinalIgnoreCase))
        {
            return rows
                .GroupBy(x =>
                {
                    var local = CostaRicaTime.ToLocalDate(x.CreatedAt);
                    return new { local.Year, local.Month };
                })
                .OrderBy(x => x.Key.Year)
                .ThenBy(x => x.Key.Month)
                .Select(x => new SalesTrendPointDto
                {
                    Date = $"{x.Key.Year:D4}-{x.Key.Month:D2}",
                    Ingresos = x.Sum(s => s.Ingresos),
                    Ganancia = x.Sum(s => s.TotalProfit)
                })
                .ToList();
        }

        return rows
            .GroupBy(x => CostaRicaTime.ToLocalDate(x.CreatedAt))
            .OrderBy(x => x.Key)
            .Select(x => new SalesTrendPointDto
            {
                Date = x.Key.ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture),
                Ingresos = x.Sum(s => s.Ingresos),
                Ganancia = x.Sum(s => s.TotalProfit)
            })
            .ToList();
    }

    public async Task<TicketAverageDto> GetTicketAverageAsync(
        Guid businessId,
        DateTime? from,
        DateTime? toExclusive)
    {
        var salesQuery = _context.Sales
            .AsNoTracking()
            .Where(x => x.BusinessId == businessId);

        if (from.HasValue)
            salesQuery = salesQuery.Where(x => x.CreatedAt >= from.Value);

        if (toExclusive.HasValue)
            salesQuery = salesQuery.Where(x => x.CreatedAt < toExclusive.Value);

        var average = await salesQuery
            .Select(x => (decimal?)(x.Total > 0 ? x.Total : x.TotalAmount))
            .AverageAsync() ?? 0m;

        return new TicketAverageDto
        {
            AverageTicket = average
        };
    }

    public async Task<List<ActivityItemDto>> GetRecentActivityAsync(
        Guid businessId,
        int limit,
        DateTime? fromUtcInclusive,
        DateTime? toUtcExclusive)
    {
        var perSource = Math.Min(Math.Max(limit * 4, 40), 300);

        var saleQuery = _context.Sales
            .AsNoTracking()
            .Where(x => x.BusinessId == businessId);
        if (fromUtcInclusive.HasValue)
            saleQuery = saleQuery.Where(x => x.CreatedAt >= fromUtcInclusive.Value);
        if (toUtcExclusive.HasValue)
            saleQuery = saleQuery.Where(x => x.CreatedAt < toUtcExclusive.Value);

        var saleActivities = await saleQuery
            .OrderByDescending(x => x.CreatedAt)
            .Take(perSource)
            .Select(x => new ActivityItemDto
            {
                Type = "sale_created",
                Description = $"Venta {x.InvoiceNumber} creada",
                CreatedAt = x.CreatedAt
            })
            .ToListAsync();

        var orderQuery = _context.RepairOrders
            .AsNoTracking()
            .Where(x => x.BusinessId == businessId
                        && x.Status == (int)RepairOrderStatus.Delivered);
        if (fromUtcInclusive.HasValue)
            orderQuery = orderQuery.Where(x => x.UpdatedAt >= fromUtcInclusive.Value);
        if (toUtcExclusive.HasValue)
            orderQuery = orderQuery.Where(x => x.UpdatedAt < toUtcExclusive.Value);

        var orderActivities = await orderQuery
            .OrderByDescending(x => x.UpdatedAt)
            .Take(perSource)
            .Select(x => new ActivityItemDto
            {
                Type = "order_delivered",
                Description = $"Orden {x.OrderNumber} entregada",
                CreatedAt = x.UpdatedAt
            })
            .ToListAsync();

        return saleActivities
            .Concat(orderActivities)
            .OrderByDescending(x => x.CreatedAt)
            .Take(limit)
            .ToList();
    }

    public async Task<List<TopProductRowDto>> GetTopProductsAsync(
        Guid businessId,
        int take,
        DateTime? fromUtcInclusive,
        DateTime? toUtcExclusive)
    {
        take = Math.Clamp(take, 1, 100);

        var salesInRange = _context.Sales
            .AsNoTracking()
            .Where(s => s.BusinessId == businessId);
        if (fromUtcInclusive.HasValue)
            salesInRange = salesInRange.Where(s => s.CreatedAt >= fromUtcInclusive.Value);
        if (toUtcExclusive.HasValue)
            salesInRange = salesInRange.Where(s => s.CreatedAt < toUtcExclusive.Value);

        return await (
                from si in _context.SaleItems.AsNoTracking()
                join s in salesInRange on si.SaleId equals s.Id
                join v in _context.CatalogVariants.AsNoTracking() on si.CatalogVariantId equals v.Id
                join ci in _context.CatalogItems.AsNoTracking() on v.CatalogItemId equals ci.Id
                where si.ItemType == "Product"
                      && si.CatalogVariantId != null
                group new { si.Quantity, si.Total } by ci.Name into g
                orderby g.Sum(x => x.Total) descending
                select new TopProductRowDto
                {
                    Name = g.Key ?? string.Empty,
                    TotalSold = g.Sum(x => x.Quantity),
                    TotalRevenue = g.Sum(x => x.Total)
                })
            .Take(take)
            .ToListAsync();
    }

    public async Task<List<PendingOrderRowDto>> GetPendingOrdersWithBalanceAsync(Guid businessId)
    {
        var orders = await _context.RepairOrders
            .AsNoTracking()
            .Where(o => o.BusinessId == businessId
                        && o.IsActive
                        && o.Status != (int)RepairOrderStatus.Cancelled)
            .Include(o => o.Items)
            .Include(o => o.Contact)
            .ToListAsync();

        if (orders.Count == 0)
            return new List<PendingOrderRowDto>();

        var orderIds = orders.Select(o => o.Id).ToList();
        var paidRows = await _context.Payments
            .AsNoTracking()
            .Where(p => p.BusinessId == businessId && orderIds.Contains(p.RepairOrderId))
            .GroupBy(p => p.RepairOrderId)
            .Select(g => new { RepairOrderId = g.Key, Sum = g.Sum(x => x.Amount) })
            .ToListAsync();

        var paidByOrder = paidRows.ToDictionary(x => x.RepairOrderId, x => x.Sum);

        var result = new List<PendingOrderRowDto>();
        foreach (var o in orders)
        {
            var totalOrden = ComputeRepairOrderTotal(o);
            var paid = paidByOrder.GetValueOrDefault(o.Id, 0m);
            var saldo = Math.Max(0m, totalOrden - paid);
            if (saldo <= 0)
                continue;

            result.Add(new PendingOrderRowDto
            {
                OrderId = o.Id,
                OrderNumber = o.OrderNumber,
                CustomerName = o.Contact?.Name?.Trim() ?? string.Empty,
                PendingAmount = Math.Round(saldo, 2, MidpointRounding.AwayFromZero)
            });
        }

        return result
            .OrderByDescending(x => x.PendingAmount)
            .ThenBy(x => x.OrderNumber)
            .ToList();
    }

    public async Task<ProfitBySourceDto> GetProfitBySourceAsync(
        Guid businessId,
        DateTime? fromUtcInclusive,
        DateTime? toUtcExclusive)
    {
        var salesQuery = _context.Sales
            .AsNoTracking()
            .Where(s => s.BusinessId == businessId);
        if (fromUtcInclusive.HasValue)
            salesQuery = salesQuery.Where(s => s.CreatedAt >= fromUtcInclusive.Value);
        if (toUtcExclusive.HasValue)
            salesQuery = salesQuery.Where(s => s.CreatedAt < toUtcExclusive.Value);

        var rows = await salesQuery
            .Select(s => new { s.RepairOrderId, s.Source, s.TotalProfit })
            .ToListAsync();

        var dto = new ProfitBySourceDto();
        foreach (var r in rows)
        {
            if (r.RepairOrderId.HasValue)
            {
                dto.Repair += r.TotalProfit;
                continue;
            }

            var src = r.Source?.Trim() ?? string.Empty;
            if (string.Equals(src, "WhatsApp", StringComparison.OrdinalIgnoreCase))
                dto.Whatsapp += r.TotalProfit;
            else
                dto.Manual += r.TotalProfit;
        }

        return dto;
    }

    private static decimal ComputeRepairOrderTotal(RepairOrder order)
    {
        return order.Items?.Sum(x => x.Price * x.Quantity) ?? 0m;
    }
}
