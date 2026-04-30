using Microsoft.EntityFrameworkCore;
using MiNegocioCR.Api.Application.Interfaces;
using MiNegocioCR.Api.Application.Interfaces.RepairOrders;
using MiNegocioCR.Api.Application.Interfaces.Services;
using MiNegocioCR.Api.Domain.Entities;
using MiNegocioCR.Api.Domain.Enums;
using MiNegocioCR.Api.Domain.Exceptions;

namespace MiNegocioCR.Api.Application.UseCases.RepairOrder;

public class ChargeRepairOrderUseCase : IChargeRepairOrderUseCase
{
    private readonly IAppDbContext _context;
    private readonly IInventoryService _inventoryService;

    public ChargeRepairOrderUseCase(IAppDbContext context, IInventoryService inventoryService)
    {
        _context = context;
        _inventoryService = inventoryService;
    }

    public async Task<object> Execute(Guid businessId, Guid repairOrderId, decimal? taxRatePercent = null)
    {
        if (businessId == Guid.Empty) throw new ArgumentException("BusinessId is required.", nameof(businessId));
        var ratePercent = taxRatePercent ?? 13m;
        if (ratePercent < 0) throw new ArgumentException("Tax rate cannot be negative.", nameof(taxRatePercent));

        var repairOrder = await _context.RepairOrders
            .Include(x => x.Contact)
            .Include(x => x.Items)
            .FirstOrDefaultAsync(x => x.BusinessId == businessId && x.Id == repairOrderId);

        if (repairOrder is null)
            throw new NotFoundException("RepairOrder", "Repair order not found.");

        if (repairOrder.IsInvoiced)
            throw new InvalidOperationException("Repair order is already invoiced.");

        if (repairOrder.Items.Count == 0)
            throw new InvalidOperationException("Repair order does not have billable items.");

        var alreadyExists = await _context.Sales
            .AnyAsync(x => x.BusinessId == businessId && x.RepairOrderId == repairOrderId);
        if (alreadyExists)
            throw new InvalidOperationException("A sale already exists for this repair order.");

        using var tx = await _context.Database.BeginTransactionAsync();

        var now = DateTime.UtcNow;
        var sale = new Sale
        {
            Id = Guid.NewGuid(),
            BusinessId = businessId,
            ContactId = repairOrder.ContactId,
            Contact = repairOrder.Contact,
            RepairOrderId = repairOrder.Id,
            InvoiceNumber = await BuildInvoiceNumberAsync(businessId),
            Source = "Repair",
            SaleDate = now,
            CreatedAt = now,
            CustomerPhone = repairOrder.Contact?.Phone ?? string.Empty,
            PayCash = repairOrder.PayCash,
            PayTransfer = repairOrder.PayTransfer,
            PaySinpe = repairOrder.PaySinpe,
            PayCard = repairOrder.PayCard
        };

        foreach (var item in repairOrder.Items)
        {
            if (item.Quantity <= 0)
                throw new InvalidOperationException("Repair order contains items with invalid quantity.");
            if (item.Price < 0)
                throw new InvalidOperationException("Repair order contains items with invalid price.");

            var itemType = item.CatalogVariantId.HasValue ? "Product" : "Service";
            if (itemType == "Service" && string.IsNullOrWhiteSpace(item.Description))
                throw new InvalidOperationException("Service items require description.");

            if (itemType == "Product")
            {
                var variantId = item.CatalogVariantId!.Value;
                var belongsToBusiness = await _context.CatalogVariants
                    .AnyAsync(v => v.Id == variantId && v.CatalogItem.BusinessId == businessId);
                if (!belongsToBusiness)
                    throw new InvalidOperationException("Repair order has product items outside business scope.");

                await _inventoryService.DecreaseStockAsync(
                    businessId,
                    variantId,
                    item.Quantity,
                    $"Repair charge {repairOrder.OrderNumber}");
            }

            sale.Items.Add(new SaleItem
            {
                Id = Guid.NewGuid(),
                SaleId = sale.Id,
                CatalogVariantId = item.CatalogVariantId,
                Description = string.IsNullOrWhiteSpace(item.Description) ? null : item.Description.Trim(),
                ItemType = itemType,
                Quantity = item.Quantity,
                Price = item.Price,
                UnitPrice = item.Price,
                Total = item.Price * item.Quantity
            });
        }

        sale.Subtotal = sale.Items.Sum(x => x.UnitPrice * x.Quantity);
        sale.DiscountAmount = Math.Round(sale.Subtotal * (repairOrder.DiscountPercent / 100m), 2, MidpointRounding.AwayFromZero);
        var taxableBase = sale.Subtotal - sale.DiscountAmount;
        sale.TaxAmount = Math.Round(taxableBase * (ratePercent / 100m), 2, MidpointRounding.AwayFromZero);
        sale.Total = taxableBase + sale.TaxAmount;
        sale.TotalAmount = sale.Total;

        _context.Sales.Add(sale);

        repairOrder.IsInvoiced = true;
        repairOrder.InvoicedAt = now;
        repairOrder.SaleId = sale.Id;
        repairOrder.Status = (int)RepairOrderStatus.Delivered;
        repairOrder.UpdatedAt = now;

        await _context.SaveChangesAsync(CancellationToken.None);
        await tx.CommitAsync();

        return new
        {
            sale.Id,
            sale.InvoiceNumber,
            sale.RepairOrderId,
            Totals = new
            {
                sale.Subtotal,
                Tax = sale.TaxAmount,
                Discount = sale.DiscountAmount,
                sale.Total
            }
        };
    }

    private async Task<string> BuildInvoiceNumberAsync(Guid businessId)
    {
        var today = DateTime.UtcNow.Date;
        var prefix = $"FACT-{today:yyyyMMdd}-";
        var numbers = await _context.Sales
            .Where(s => s.BusinessId == businessId
                && s.InvoiceNumber != null
                && s.InvoiceNumber.StartsWith(prefix))
            .Select(s => s.InvoiceNumber)
            .ToListAsync();

        var seq = 1;
        if (numbers.Count > 0)
        {
            var max = numbers
                .Select(x => int.TryParse(x[^4..], out var n) ? n : 0)
                .DefaultIfEmpty(0)
                .Max();
            seq = max + 1;
        }

        return $"{prefix}{seq:D4}";
    }
}
