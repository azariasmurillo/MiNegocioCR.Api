using MiNegocioCR.Api.Application.Interfaces.Repositories;
using MiNegocioCR.Api.Application.Interfaces.Services;
using MiNegocioCR.Api.Application.Interfaces;
using MiNegocioCR.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace MiNegocioCR.Api.Application.UseCases.Repository
{
    public class RegisterSaleUseCase
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

        public RegisterSaleUseCase(
            ISaleRepository saleRepository,
            IInventoryService inventoryService)
        {
            _saleRepository = saleRepository;
            _inventoryService = inventoryService;
            _context = null!;
        }

        public async Task<Guid> ExecuteAsync(
            Guid businessId,
            List<(Guid variantId, int quantity, decimal price)> items)
        {
            if (items == null || !items.Any()) throw new ArgumentException("At least one item is required.", nameof(items));

            var now = DateTime.UtcNow;
            var sale = new Sale
            {
                Id = Guid.NewGuid(),
                BusinessId = businessId,
                InvoiceNumber = await BuildInvoiceNumberAsync(businessId),
                Source = "Manual",
                SaleDate = now,
                CreatedAt = now,
                CustomerPhone = string.Empty
            };

            foreach (var item in items)
            {
                var lineTotal = item.price * item.quantity;
                sale.Items.Add(new SaleItem
                {
                    Id = Guid.NewGuid(),
                    CatalogVariantId = item.variantId,
                    ItemType = "Product",
                    Quantity = item.quantity,
                    Price = item.price,
                    UnitPrice = item.price,
                    Total = lineTotal
                });

                await _inventoryService.DecreaseStockAsync(
                    businessId,
                    item.variantId,
                    item.quantity,
                    "Sale"
                );
            }

            sale.Subtotal = sale.Items.Sum(x => x.UnitPrice * x.Quantity);
            sale.TaxAmount = 0m;
            sale.DiscountAmount = 0m;
            sale.Total = sale.Subtotal + sale.TaxAmount - sale.DiscountAmount;
            sale.TotalAmount = sale.Total;

            await _saleRepository.AddSaleAsync(sale);

            return sale.Id;
        }

        private async Task<string> BuildInvoiceNumberAsync(Guid businessId)
        {
            if (_context == null)
            {
                return $"FACT-{DateTime.UtcNow:yyyyMMdd}-{Random.Shared.Next(1, 9999):D4}";
            }

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
}
