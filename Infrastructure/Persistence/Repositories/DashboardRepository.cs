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

    public async Task<DashboardSummaryDto> GetSummaryAsync(Guid businessId, DateTime? from, DateTime? to)
    {
        _ = from;
        _ = to;

        var startToday = CostaRicaTime.ToUtcStartOfDay(CostaRicaTime.Today);
        var endToday = CostaRicaTime.ToUtcEndExclusive(CostaRicaTime.Today);

        var salesToday = _context.Sales
            .AsNoTracking()
            .Where(x => x.BusinessId == businessId
                        && x.CreatedAt >= startToday
                        && x.CreatedAt < endToday);

        var salesTodayCount = await salesToday.CountAsync();
        var ingresosHoy = await salesToday
            .Select(x => (decimal?)(x.Total > 0 ? x.Total : x.TotalAmount))
            .SumAsync() ?? 0m;

        var gananciaHoy = await salesToday
            .Select(x => (decimal?)x.TotalProfit)
            .SumAsync() ?? 0m;

        var ticketPromedio = salesTodayCount > 0
            ? Math.Round(ingresosHoy / salesTodayCount, 2, MidpointRounding.AwayFromZero)
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

    public async Task<List<ActivityItemDto>> GetRecentActivityAsync(Guid businessId, int limit)
    {
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

    public async Task<List<TopProductRowDto>> GetTopProductsAsync(Guid businessId, int take)
    {
        take = Math.Clamp(take, 1, 100);

        return await (
                from si in _context.SaleItems.AsNoTracking()
                join s in _context.Sales.AsNoTracking() on si.SaleId equals s.Id
                join v in _context.CatalogVariants.AsNoTracking() on si.CatalogVariantId equals v.Id
                join ci in _context.CatalogItems.AsNoTracking() on v.CatalogItemId equals ci.Id
                where s.BusinessId == businessId
                      && si.ItemType == "Product"
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
        var business = await _context.Businesses.AsNoTracking()
            .FirstOrDefaultAsync(b => b.Id == businessId);
        if (business == null)
            return new List<PendingOrderRowDto>();

        var taxRate = business.TaxRatePercent < 0 ? 0m : business.TaxRatePercent;

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
            var totalOrden = ComputeRepairOrderTotalWithTax(o, taxRate);
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

    public async Task<ProfitBySourceDto> GetProfitBySourceAsync(Guid businessId)
    {
        var rows = await _context.Sales
            .AsNoTracking()
            .Where(s => s.BusinessId == businessId)
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

    private static decimal ComputeRepairOrderTotalWithTax(RepairOrder order, decimal taxRatePercent)
    {
        var subtotal = order.Items?.Sum(x => x.Price * x.Quantity) ?? 0m;
        var tax = Math.Round(subtotal * (taxRatePercent / 100m), 2, MidpointRounding.AwayFromZero);
        return subtotal + tax;
    }
}
