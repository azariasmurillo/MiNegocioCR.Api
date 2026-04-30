using Microsoft.EntityFrameworkCore;
using MiNegocioCR.Api.Application.Common;
using MiNegocioCR.Api.Application.DTOs;
using MiNegocioCR.Api.Application.Interfaces;
using MiNegocioCR.Api.Application.Interfaces.MiNegocioCR.Api.Application.Interfaces.UseCases.Sales;
using MiNegocioCR.Api.Application.Interfaces.Repositories;
using MiNegocioCR.Api.Application.Interfaces.Services;
using MiNegocioCR.Api.Domain.Entities;

namespace MiNegocioCR.Api.Application.UseCases.Sales
{
    public class RegisterSaleUseCase : IRegisterSaleUseCase
    {
        private readonly ISaleRepository _saleRepository;
        private readonly IInventoryService _inventoryService;
        private readonly IAppDbContext _context;

        public RegisterSaleUseCase(
            ISaleRepository saleRepository,
            IInventoryService inventoryService,
            IAppDbContext context)
        {
            _saleRepository = saleRepository;
            _inventoryService = inventoryService;
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
            if (items == null || !items.Any()) throw new ArgumentException("At least one item is required.", nameof(items));
            if (request.TaxRatePercent < 0) throw new ArgumentException("Tax rate cannot be negative.", nameof(request.TaxRatePercent));
            if (request.Discount < 0) throw new ArgumentException("Discount cannot be negative.", nameof(request.Discount));

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                MiNegocioCR.Api.Domain.Entities.Contact? contact = null;
                if (request.ContactId.HasValue)
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
                    SaleDate = DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow,
                    CustomerPhone = phoneForSale,
                    ContactId = contact?.Id,
                    Contact = contact,
                    PayCash = request.PayCash,
                    PayTransfer = request.PayTransfer,
                    PaySinpe = request.PaySinpe,
                    PayCard = request.PayCard
                };

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

                sale.Subtotal = sale.Items.Sum(x => x.UnitPrice * x.Quantity);
                sale.TaxAmount = Math.Round(sale.Subtotal * (request.TaxRatePercent / 100m), 2, MidpointRounding.AwayFromZero);
                sale.DiscountAmount = request.Discount;
                sale.Total = sale.Subtotal + sale.TaxAmount - sale.DiscountAmount;
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
