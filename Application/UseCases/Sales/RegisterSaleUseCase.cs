using Microsoft.EntityFrameworkCore;
using MiNegocioCR.Api.Application.Common;
using MiNegocioCR.Api.Application.DTOs;
using MiNegocioCR.Api.Application.Interfaces;
using MiNegocioCR.Api.Application.Interfaces.MiNegocioCR.Api.Application.Interfaces.UseCases.Sales;
using MiNegocioCR.Api.Application.Interfaces.Repositories;
using MiNegocioCR.Api.Application.Interfaces.Services;
using MiNegocioCR.Api.Domain.Entities;
using MiNegocioCR.Api.Domain.Enums;

namespace MiNegocioCR.Api.Application.UseCases.Sales
{
    public class RegisterSaleUseCase : IRegisterSaleUseCase
    {
        private readonly ISaleRepository _saleRepository;
        private readonly IInventoryService _inventoryService;
        private readonly IPaymentService _paymentService;
        private readonly IAppDbContext _context;

        public RegisterSaleUseCase(
            ISaleRepository saleRepository,
            IInventoryService inventoryService,
            IPaymentService paymentService,
            IAppDbContext context)
        {
            _saleRepository = saleRepository;
            _inventoryService = inventoryService;
            _paymentService = paymentService;
            _context = context;
        }

        public async Task<object> ExecuteAsync(
            CreateSaleRequestDto request,
            string? customerPhone = null,
            string? customerName = null,
            string? customerEmail = null)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            var businessId = request.BusinessId;
            var items = request.Items;
            if (request.TaxRatePercent < 0) throw new ArgumentException("Tax rate cannot be negative.", nameof(request.TaxRatePercent));
            if (request.Discount < 0) throw new ArgumentException("Discount cannot be negative.", nameof(request.Discount));

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                MiNegocioCR.Api.Domain.Entities.RepairOrder? repairOrder = null;
                if (request.RepairOrderId.HasValue)
                {
                    repairOrder = await _context.RepairOrders
                        .Include(r => r.Contact)
                        .Include(r => r.Items)
                        .FirstOrDefaultAsync(r => r.BusinessId == businessId && r.Id == request.RepairOrderId.Value);

                    if (repairOrder is null)
                        throw new ArgumentException("RepairOrder does not belong to this business.");

                    if (repairOrder.IsInvoiced)
                        throw new InvalidOperationException("Repair order is already invoiced.");

                    if ((RepairOrderStatus)repairOrder.Status == RepairOrderStatus.Cancelled)
                        throw new InvalidOperationException("Cannot create sale for cancelled repair order.");

                    var exists = await _context.Sales
                        .AnyAsync(s => s.BusinessId == businessId && s.RepairOrderId == request.RepairOrderId.Value);
                    if (exists)
                        throw new InvalidOperationException("A sale already exists for this repair order.");
                }

                MiNegocioCR.Api.Domain.Entities.Contact? contact = null;
                if (repairOrder != null)
                {
                    contact = repairOrder.Contact;
                }
                else if (request.ContactId.HasValue)
                {
                    contact = await _context.Contacts
                        .FirstOrDefaultAsync(c => c.Id == request.ContactId.Value && c.BusinessId == businessId);
                    if (contact == null)
                        throw new ArgumentException("Contact does not belong to this business.");
                }
                else
                {
                    contact = await SaleContactResolution.TryResolveOrCreateAsync(
                        _context,
                        businessId,
                        customerPhone ?? request.CustomerPhone,
                        customerName ?? request.CustomerName,
                        customerEmail ?? request.CustomerEmail);
                }

                var phoneForSale = contact != null
                    ? contact.Phone
                    : ((customerPhone ?? request.CustomerPhone)?.Trim() ?? string.Empty);

                var sale = new Sale
                {
                    Id = Guid.NewGuid(),
                    BusinessId = businessId,
                    InvoiceNumber = await BuildInvoiceNumberAsync(businessId),
                    Source = NormalizeSource(request.Source),
                    RepairOrderId = request.RepairOrderId,
                    SaleDate = DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow,
                    CustomerPhone = phoneForSale,
                    ContactId = contact?.Id,
                    Contact = contact,
                    PayCash = repairOrder?.PayCash ?? request.PayCash,
                    PayTransfer = repairOrder?.PayTransfer ?? request.PayTransfer,
                    PaySinpe = repairOrder?.PaySinpe ?? request.PaySinpe,
                    PayCard = repairOrder?.PayCard ?? request.PayCard
                };

                if (repairOrder != null)
                {
                    if (repairOrder.Items == null || repairOrder.Items.Count == 0)
                        throw new ArgumentException("Repair order does not have billable items.");

                    foreach (var item in repairOrder.Items)
                    {
                        if (item.Quantity <= 0)
                            throw new ArgumentException("Item quantity must be greater than zero.");
                        if (item.Price < 0)
                            throw new ArgumentException("Item unit price cannot be negative.");

                        var normalizedType = item.CatalogVariantId.HasValue ? "Product" : "Service";
                        if (normalizedType == "Product")
                        {
                            var variantId = item.CatalogVariantId ?? throw new ArgumentException("CatalogVariantId is required for Product items.");
                            var variantBelongsToBusiness = await _context.CatalogVariants
                                .AnyAsync(v => v.Id == variantId && v.CatalogItem.BusinessId == businessId);
                            if (!variantBelongsToBusiness)
                                throw new ArgumentException("CatalogVariantId does not belong to this business.");

                            await _inventoryService.DecreaseStockAsync(
                                businessId,
                                variantId,
                                item.Quantity,
                                $"Repair charge {repairOrder.OrderNumber}");
                        }
                        else if (string.IsNullOrWhiteSpace(item.Description))
                        {
                            throw new ArgumentException("Description is required for Service items.");
                        }

                        sale.Items.Add(new SaleItem
                        {
                            Id = Guid.NewGuid(),
                            CatalogVariantId = item.CatalogVariantId,
                            Description = string.IsNullOrWhiteSpace(item.Description) ? null : item.Description.Trim(),
                            ItemType = normalizedType,
                            Quantity = item.Quantity,
                            Price = item.Price,
                            UnitPrice = item.Price,
                            Total = item.Price * item.Quantity
                        });
                    }
                }
                else
                {
                    if (items == null || !items.Any()) throw new ArgumentException("At least one item is required.", nameof(items));
                    foreach (var item in items)
                    {
                        if (item.Quantity <= 0)
                            throw new ArgumentException("Item quantity must be greater than zero.");
                        if (item.UnitPrice < 0)
                            throw new ArgumentException("Item unit price cannot be negative.");

                        var normalizedType = NormalizeItemType(item.ItemType);
                        if (normalizedType == "Product")
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
                                "Sale");
                        }
                        else if (string.IsNullOrWhiteSpace(item.Description))
                        {
                            throw new ArgumentException("Description is required for Service items.");
                        }

                        sale.Items.Add(new SaleItem
                        {
                            Id = Guid.NewGuid(),
                            CatalogVariantId = item.CatalogVariantId,
                            Description = string.IsNullOrWhiteSpace(item.Description) ? null : item.Description.Trim(),
                            ItemType = normalizedType,
                            Quantity = item.Quantity,
                            Price = item.UnitPrice,
                            UnitPrice = item.UnitPrice,
                            Total = item.UnitPrice * item.Quantity
                        });
                    }
                }

                sale.Subtotal = sale.Items.Sum(x => x.UnitPrice * x.Quantity);
                if (repairOrder != null)
                {
                    // Mismo criterio que el antiguo /charge: % sobre subtotal de líneas, IVA sobre base gravable.
                    var discountPercentAmount = Math.Round(
                        sale.Subtotal * (repairOrder.DiscountPercent / 100m), 2, MidpointRounding.AwayFromZero);
                    if (discountPercentAmount > sale.Subtotal)
                        discountPercentAmount = sale.Subtotal;

                    var taxableBase = sale.Subtotal - discountPercentAmount;
                    sale.TaxAmount = Math.Round(
                        taxableBase * (request.TaxRatePercent / 100m), 2, MidpointRounding.AwayFromZero);

                    // Payments: única fuente de prepagos; no usar montos del request.
                    var payments = await _paymentService.GetPaymentsByRepairOrderAsync(businessId, repairOrder.Id);
                    var totalPagado = payments.Sum(p => p.Amount);

                    var totalOrden = taxableBase + sale.TaxAmount;

                    if (totalPagado > totalOrden)
                        throw new InvalidOperationException("La suma de pagos supera el total de la orden.");

                    if (totalPagado >= totalOrden)
                        throw new InvalidOperationException(
                            "No se puede crear la venta: la orden ya está pagada en su totalidad (no hay saldo pendiente por facturar).");

                    var saldoPendiente = totalOrden - totalPagado;

                    // DiscountAmount = descuento % de la orden + pagos ya registrados (sin duplicar filas en Payments).
                    sale.DiscountAmount = discountPercentAmount + totalPagado;
                    sale.Total = saldoPendiente;

                    repairOrder.IsInvoiced = true;
                    repairOrder.InvoicedAt = DateTime.UtcNow;
                    repairOrder.SaleId = sale.Id;
                    repairOrder.Status = (int)RepairOrderStatus.Delivered;
                    repairOrder.UpdatedAt = DateTime.UtcNow;
                }
                else
                {
                    sale.TaxAmount = Math.Round(
                        sale.Subtotal * (request.TaxRatePercent / 100m), 2, MidpointRounding.AwayFromZero);
                    sale.DiscountAmount = request.Discount;
                    sale.Total = sale.Subtotal + sale.TaxAmount - sale.DiscountAmount;
                }
                sale.TotalAmount = sale.Total;

                await _saleRepository.AddSaleAsync(sale);

                await transaction.CommitAsync();

                return new
                {
                    sale.Id,
                    sale.InvoiceNumber,
                    sale.Source,
                    Contact = contact == null ? null : new
                    {
                        contact.Id,
                        contact.Name,
                        contact.Phone,
                        contact.Email
                    },
                    Items = sale.Items.Select(i => new
                    {
                        i.Id,
                        i.ItemType,
                        i.CatalogVariantId,
                        i.Description,
                        i.Quantity,
                        i.UnitPrice,
                        LineTotal = i.UnitPrice * i.Quantity
                    }),
                    Totals = new
                    {
                        sale.Subtotal,
                        Tax = sale.TaxAmount,
                        Discount = sale.DiscountAmount,
                        sale.Total
                    }
                };
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
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

        private static string NormalizeItemType(string? value)
        {
            return string.Equals(value, "Product", StringComparison.OrdinalIgnoreCase) ? "Product" : "Service";
        }

        private static string NormalizeSource(string? value)
        {
            if (string.Equals(value, "Repair", StringComparison.OrdinalIgnoreCase)) return "Repair";
            if (string.Equals(value, "WhatsApp", StringComparison.OrdinalIgnoreCase)) return "WhatsApp";
            return "Manual";
        }
    }
}
