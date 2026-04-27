using Microsoft.EntityFrameworkCore;
using MiNegocioCR.Api.Application.Common;
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

        public async Task<Guid> ExecuteAsync(
            Guid businessId,
            List<(Guid variantId, int quantity, decimal price)> items,
            string? customerPhone = null,
            string? customerName = null,
            string? customerEmail = null)
        {
            if (items == null || !items.Any()) throw new ArgumentException("At least one item is required.", nameof(items));

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var contact = await SaleContactResolution.TryResolveOrCreateAsync(
                    _context,
                    businessId,
                    customerPhone,
                    customerName,
                    customerEmail);

                var phoneForSale = contact != null
                    ? contact.Phone
                    : (customerPhone?.Trim() ?? string.Empty);

                var sale = new Sale
                {
                    Id = Guid.NewGuid(),
                    BusinessId = businessId,
                    InvoiceNumber = await BuildInvoiceNumberAsync(businessId),
                    SaleDate = DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow,
                    CustomerPhone = phoneForSale,
                    ContactId = contact?.Id,
                    Contact = contact
                };

                foreach (var item in items)
                {
                    sale.Items.Add(new SaleItem
                    {
                        Id = Guid.NewGuid(),
                        CatalogVariantId = item.variantId,
                        ItemType = "Product",
                        Quantity = item.quantity,
                        Price = item.price
                    });

                    await _inventoryService.DecreaseStockAsync(
                        businessId,
                        item.variantId,
                        item.quantity,
                        "Sale");
                }

                sale.Total = sale.Items.Sum(x => x.Price * x.Quantity);
                sale.TotalAmount = sale.Total;

                await _saleRepository.AddSaleAsync(sale);

                await transaction.CommitAsync();

                return sale.Id;
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
    }
}
