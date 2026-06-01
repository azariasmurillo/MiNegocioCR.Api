using MiNegocioCR.Api.Application.Common;
using Microsoft.EntityFrameworkCore;
using MiNegocioCR.Api.Application.DTOs;
using MiNegocioCR.Api.Application.Interfaces;
using MiNegocioCR.Api.Application.Interfaces.UseCases.Sales;
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
            if (request.Discount < 0) throw new ArgumentException("Discount cannot be negative.", nameof(request.Discount));
            if (request.DiscountValue < 0) throw new ArgumentException("Discount value cannot be negative.", nameof(request.DiscountValue));

            var businessId = request.BusinessId;

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var business = await _context.Businesses.AsNoTracking()
                    .FirstOrDefaultAsync(b => b.Id == businessId);
                if (business == null)
                    throw new ArgumentException("Business not found.", nameof(request.BusinessId));

                var taxRate = business.TaxRatePercent;
                if (taxRate < 0)
                    throw new ArgumentException("Business tax rate cannot be negative.");

                // ── Cargar orden de reparación (si aplica) ─────────────────
                Domain.Entities.RepairOrder? repairOrder = null;
                if (request.RepairOrderId.HasValue)
                {
                    repairOrder = await _context.RepairOrders
                        .AsTracking()
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

                // ── Resolver contacto ──────────────────────────────────────
                Domain.Entities.Contact? contact = null;
                if (repairOrder != null)
                {
                    contact = repairOrder.Contact;
                }
                else if (request.ContactId.HasValue)
                {
                    contact = await _context.Contacts
                        .AsTracking()
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

                ApplyContactDetailsFromRequest(
                    contact,
                    customerName ?? request.CustomerName,
                    customerEmail ?? request.CustomerEmail);

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
                };

                // ── Construir ítems ────────────────────────────────────────
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
                            var variantId = item.CatalogVariantId!.Value;
                            var variantBelongs = await _context.CatalogVariants
                                .AnyAsync(v => v.Id == variantId && v.CatalogItem.BusinessId == businessId);
                            if (!variantBelongs)
                                throw new ArgumentException("CatalogVariantId does not belong to this business.");

                            await _inventoryService.DecreaseStockAsync(
                                businessId, variantId, item.Quantity,
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
                    var items = request.Items;
                    if (items == null || !items.Any())
                        throw new ArgumentException("At least one item is required.", nameof(items));

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

                            var variantBelongs = await _context.CatalogVariants
                                .AnyAsync(v => v.Id == item.CatalogVariantId.Value && v.CatalogItem.BusinessId == businessId);
                            if (!variantBelongs)
                                throw new ArgumentException("CatalogVariantId does not belong to this business.");

                            await _inventoryService.DecreaseStockAsync(
                                businessId, item.CatalogVariantId.Value, item.Quantity, "Sale");
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

                // ── Calcular totales (unificado: directo y reparación) ─────
                sale.Subtotal = sale.Items.Sum(x => x.UnitPrice * x.Quantity);

                var (discountKind, discountInput, discountAmount) = SaleDiscountCalculator.Resolve(
                    sale.Subtotal,
                    request.DiscountKind,
                    request.DiscountValue,
                    request.Discount);

                var (taxAmount, totalOrden) = SaleDiscountCalculator.ComputeTotals(
                    sale.Subtotal, discountAmount, taxRate);

                sale.DiscountAmount = discountAmount;
                sale.DiscountKind = (byte)discountKind;
                sale.DiscountInputValue = discountInput;
                sale.TaxAmount = taxAmount;
                sale.TotalOrden = totalOrden;

                if (repairOrder != null)
                {
                    var payments = await _paymentService.GetPaymentsByRepairOrderAsync(businessId, repairOrder.Id);
                    var prepaidAmount = payments.Sum(p => p.Amount);

                    // Abonos pueden superar TotalOrden cuando el descuento se aplica al facturar
                    // (p. ej. donación/cortesía tras abonos parciales). Saldo cobrado hoy = max(0, …).
                    var saldoPendiente = Math.Max(0m, totalOrden - prepaidAmount);

                    sale.PrepaidAmount = prepaidAmount;
                    sale.Total = saldoPendiente;

                    repairOrder.IsInvoiced = true;
                    repairOrder.InvoicedAt = DateTime.UtcNow;
                    repairOrder.SaleId = sale.Id;
                    repairOrder.Status = (int)RepairOrderStatus.Delivered;
                    repairOrder.UpdatedAt = DateTime.UtcNow;
                }
                else
                {
                    sale.PrepaidAmount = 0m;
                    sale.Total = totalOrden;
                }

                sale.TotalAmount = sale.Total;

                // ── Métodos de pago con monto real ─────────────────────────
                foreach (var pm in request.PaymentMethods ?? [])
                {
                    if (pm.Amount < 0)
                        throw new ArgumentException("Payment method amount cannot be negative.");

                    var method = ParsePaymentMethod(pm.Method);
                    sale.PaymentMethods.Add(new SalePaymentMethod
                    {
                        Id = Guid.NewGuid(),
                        SaleId = sale.Id,
                        Method = method,
                        Amount = pm.Amount
                    });
                }

                var paidToday = sale.PaymentMethods.Sum(pm => pm.Amount);
                if (sale.Total > 0m && paidToday <= 0m)
                    throw new ArgumentException(
                        "Se requiere al menos un método de pago cuando el total a cobrar es mayor a cero.");

                // ── Snapshot de costos y métricas ─────────────────────────
                await ApplyCostSnapshotAndTotalsAsync(sale);

                if (contact != null && ContactActivityHelper.SaleInvolvesPayment(sale))
                    ContactActivityHelper.Touch(contact, sale.SaleDate);

                await _saleRepository.AddSaleAsync(sale);

                await transaction.CommitAsync();

                return BuildResponse(sale, contact, taxRate);
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        // ── Helpers ────────────────────────────────────────────────────────

        private static void ApplyContactDetailsFromRequest(
            Domain.Entities.Contact? contact,
            string? customerName,
            string? customerEmail)
        {
            if (contact == null)
                return;

            if (!string.IsNullOrWhiteSpace(customerName))
                contact.Name = customerName.Trim();

            if (customerEmail != null)
                contact.Email = string.IsNullOrWhiteSpace(customerEmail) ? null : customerEmail.Trim();
        }

        private static object BuildResponse(Sale sale, Domain.Entities.Contact? contact, decimal taxRate) =>
            new
            {
                sale.Id,
                sale.InvoiceNumber,
                sale.Source,
                taxRatePercent = taxRate,
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
                    Discount = sale.DiscountAmount,
                    DiscountKind = ((SaleDiscountKind)sale.DiscountKind).ToString(),
                    DiscountInputValue = sale.DiscountInputValue,
                    Tax = sale.TaxAmount,
                    TotalOrden = sale.TotalOrden,
                    PrepaidAmount = sale.PrepaidAmount,
                    Total = sale.Total
                },
                PaymentMethods = sale.PaymentMethods.Select(pm => new
                {
                    Method = pm.Method.ToString(),
                    pm.Amount
                })
            };

        private async Task ApplyCostSnapshotAndTotalsAsync(Sale sale)
        {
            var variantIds = sale.Items
                .Where(i => i.CatalogVariantId.HasValue)
                .Select(i => i.CatalogVariantId!.Value)
                .Distinct()
                .ToList();

            if (variantIds.Count > 0)
            {
                var costs = await _context.CatalogVariants.AsNoTracking()
                    .Where(v => variantIds.Contains(v.Id))
                    .ToDictionaryAsync(v => v.Id, v => v.CostPrice);

                foreach (var line in sale.Items)
                {
                    if (line.CatalogVariantId.HasValue &&
                        costs.TryGetValue(line.CatalogVariantId.Value, out var unitCost))
                        line.CostPrice = unitCost;
                }
            }

            sale.TotalCost = sale.Items.Sum(i => i.CostPrice * i.Quantity);
            // Ganancia neta sin IVA (pass-through): (TotalOrden − TaxAmount) − costo
            sale.TotalProfit = sale.TotalOrden - sale.TaxAmount - sale.TotalCost;
        }

        private async Task<string> BuildInvoiceNumberAsync(Guid businessId)
        {
            var today = DateTime.UtcNow.Date;
            var prefix = $"FACT-{today:yyyyMMdd}-";
            var existing = await _context.Sales
                .Where(s => s.BusinessId == businessId
                    && s.InvoiceNumber != null
                    && s.InvoiceNumber.StartsWith(prefix))
                .Select(s => s.InvoiceNumber)
                .ToListAsync();

            var seq = 1;
            if (existing.Count > 0)
            {
                var max = existing
                    .Select(x => int.TryParse(x[^4..], out var n) ? n : 0)
                    .DefaultIfEmpty(0)
                    .Max();
                seq = max + 1;
            }

            return $"{prefix}{seq:D4}";
        }

        private static string NormalizeItemType(string? value) =>
            string.Equals(value, "Product", StringComparison.OrdinalIgnoreCase) ? "Product" : "Service";

        private static string NormalizeSource(string? value)
        {
            if (string.Equals(value, "Repair",     StringComparison.OrdinalIgnoreCase)) return "Repair";
            if (string.Equals(value, "FromRepair", StringComparison.OrdinalIgnoreCase)) return "Repair";
            if (string.Equals(value, "WhatsApp",   StringComparison.OrdinalIgnoreCase)) return "WhatsApp";
            return "Manual";
        }

        private static PaymentMethod ParsePaymentMethod(string value)
        {
            return value.Trim().ToLowerInvariant() switch
            {
                "cash"         or "efectivo"                  => PaymentMethod.Cash,
                "transfer"     or "transferencia"             => PaymentMethod.Transfer,
                "sinpe"        or "sinpe móvil" or "sinpemovil" => PaymentMethod.Sinpe,
                "card"         or "tarjeta"                   => PaymentMethod.Card,
                _ => throw new ArgumentException($"Método de pago no reconocido: '{value}'. Valores válidos: Cash, Transfer, Sinpe, Card.")
            };
        }
    }
}
