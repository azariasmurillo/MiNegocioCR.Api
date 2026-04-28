using Microsoft.EntityFrameworkCore;
using MiNegocioCR.Api.Application.DTOs;
using MiNegocioCR.Api.Application.Interfaces;
using MiNegocioCR.Api.Application.Interfaces.MiNegocioCR.Api.Application.Interfaces.UseCases.Sales;
using MiNegocioCR.Api.Application.Interfaces.Repositories;
using MiNegocioCR.Api.Application.Interfaces.Services;
using MiNegocioCR.Api.Domain.Entities;
using MiNegocioCR.Api.Domain.Exceptions;

namespace MiNegocioCR.Api.Application.UseCases.Sales;

public class CreateSaleFromRepairUseCase : ICreateSaleFromRepairUseCase
{
    private readonly IAppDbContext _context;
    private readonly ISaleRepository _saleRepository;
    private readonly IInventoryService _inventoryService;

    public CreateSaleFromRepairUseCase(
        IAppDbContext context,
        ISaleRepository saleRepository,
        IInventoryService inventoryService)
    {
        _context = context;
        _saleRepository = saleRepository;
        _inventoryService = inventoryService;
    }

    public async Task<object> ExecuteAsync(Guid businessId, Guid repairOrderId, CreateSaleFromRepairRequestDto request)
    {
        if (request == null) throw new ArgumentNullException(nameof(request));
        if (request.Items == null || request.Items.Count == 0)
            throw new ArgumentException("At least one item is required.", nameof(request.Items));
        if (request.TaxRatePercent < 0)
            throw new ArgumentException("Tax rate cannot be negative.", nameof(request.TaxRatePercent));

        var repairOrder = await _context.RepairOrders
            .Include(r => r.Contact)
            .FirstOrDefaultAsync(r => r.BusinessId == businessId && r.Id == repairOrderId);

        if (repairOrder == null)
            throw new NotFoundException("RepairOrder", "Repair order not found.");

        if (request.PreventDuplicateInvoiceForRepair)
        {
            var exists = await _context.Sales.AnyAsync(s => s.BusinessId == businessId && s.RepairOrderId == repairOrderId);
            if (exists)
                throw new ArgumentException("A sale invoice already exists for this repair order.");
        }

        using var tx = await _context.Database.BeginTransactionAsync();

        var invoiceNumber = await BuildInvoiceNumberAsync(repairOrder.BusinessId);
        var sale = new Sale
        {
            Id = Guid.NewGuid(),
            BusinessId = repairOrder.BusinessId,
            ContactId = repairOrder.ContactId,
            Contact = repairOrder.Contact,
            RepairOrderId = repairOrder.Id,
            InvoiceNumber = invoiceNumber,
            Source = "Repair",
            SaleDate = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow,
            CustomerPhone = repairOrder.Contact.Phone
        };

        foreach (var item in request.Items)
        {
            if (item.Quantity <= 0)
                throw new ArgumentException("Item quantity must be greater than zero.");
            if (item.UnitPrice < 0)
                throw new ArgumentException("Item unit price cannot be negative.");

            var type = (item.ItemType ?? string.Empty).Trim();
            var normalized = type.Equals("Product", StringComparison.OrdinalIgnoreCase) ? "Product" : "Service";

            if (normalized == "Product")
            {
                if (!item.CatalogVariantId.HasValue)
                    throw new ArgumentException("CatalogVariantId is required for Product items.");

                var variantBelongsToBusiness = await _context.CatalogVariants
                    .AnyAsync(v => v.Id == item.CatalogVariantId.Value && v.CatalogItem.BusinessId == businessId);
                if (!variantBelongsToBusiness)
                    throw new ArgumentException("CatalogVariantId does not belong to this business.");

                await _inventoryService.DecreaseStockAsync(
                    businessId,
                    item.CatalogVariantId.Value,
                    item.Quantity,
                    $"Repair invoice {invoiceNumber}");
            }
            else if (string.IsNullOrWhiteSpace(item.Description))
            {
                throw new ArgumentException("Description is required for Service items.");
            }

            sale.Items.Add(new SaleItem
            {
                Id = Guid.NewGuid(),
                SaleId = sale.Id,
                CatalogVariantId = item.CatalogVariantId,
                ItemType = normalized,
                Description = string.IsNullOrWhiteSpace(item.Description) ? null : item.Description.Trim(),
                Quantity = item.Quantity,
                Price = item.UnitPrice,
                UnitPrice = item.UnitPrice,
                Total = item.UnitPrice * item.Quantity
            });
        }

        var subtotal = sale.Items.Sum(i => i.UnitPrice * i.Quantity);
        var tax = Math.Round(subtotal * (request.TaxRatePercent / 100m), 2, MidpointRounding.AwayFromZero);
        var total = subtotal + tax;

        sale.Subtotal = subtotal;
        sale.Tax = tax;
        sale.Discount = 0m;
        sale.Total = total;
        sale.TotalAmount = total;

        await _saleRepository.AddSaleAsync(sale);
        await tx.CommitAsync();

        return new
        {
            sale.Id,
            sale.InvoiceNumber,
            sale.RepairOrderId,
            Contact = new
            {
                sale.ContactId,
                Name = repairOrder.Contact.Name,
                Phone = repairOrder.Contact.Phone,
                Email = repairOrder.Contact.Email
            },
            Items = sale.Items.Select(i => new
            {
                i.Id,
                i.ItemType,
                i.CatalogVariantId,
                i.Description,
                i.Quantity,
                UnitPrice = i.UnitPrice,
                LineTotal = i.UnitPrice * i.Quantity
            }),
            Totals = new
            {
                Subtotal = subtotal,
                Tax = tax,
                Total = total
            }
        };
    }

    private async Task<string> BuildInvoiceNumberAsync(Guid businessId)
    {
        var today = DateTime.UtcNow.Date;
        var prefix = $"FACT-{today:yyyyMMdd}-";
        var next = await _context.Sales
            .Where(s => s.BusinessId == businessId
                && s.InvoiceNumber != null
                && s.InvoiceNumber.StartsWith(prefix))
            .Select(s => s.InvoiceNumber)
            .ToListAsync();

        var seq = 1;
        if (next.Count > 0)
        {
            var max = next
                .Select(x => int.TryParse(x[^4..], out var n) ? n : 0)
                .DefaultIfEmpty(0)
                .Max();
            seq = max + 1;
        }

        return $"{prefix}{seq:D4}";
    }
}
