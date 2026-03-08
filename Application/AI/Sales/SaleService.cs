using Microsoft.EntityFrameworkCore;
using MiNegocioCR.Api.Domain.Entities;
using MiNegocioCR.Api.Infrastructure.Persistence;

namespace MiNegocioCR.Api.Application.AI.Sales
{
    public class SaleService : ISaleService
    {
        private readonly AppDbContext _context;

        public SaleService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<string> CreateSaleAsync(
            Guid businessId,
            Guid variantId,
            string phoneNumber,
            int quantity)
        {
            var variant = await _context.CatalogVariants
                .Include(v => v.CatalogItem)
                .FirstOrDefaultAsync(v => v.Id == variantId);

            if (variant == null)
                return "Producto no encontrado.";

            if (variant.StockQuantity < quantity)
                return "No hay suficiente stock. Disponible: " + variant.StockQuantity + ".";

            var unitPrice = variant.Price;
            var totalAmount = unitPrice * quantity;

            var sale = new Sale
            {
                Id = Guid.NewGuid(),
                BusinessId = businessId,
                CreatedAt = DateTime.UtcNow,
                CustomerPhone = phoneNumber,
                TotalAmount = totalAmount,
            };

            _context.Sales.Add(sale);

            var item = new SaleItem
            {
                Id = Guid.NewGuid(),
                SaleId = sale.Id,
                CatalogVariantId = variant.Id,
                Quantity = quantity,
                UnitPrice = unitPrice,
                Total = totalAmount
            };

            _context.SaleItems.Add(item);

            variant.StockQuantity -= quantity;

            var movement = new InventoryMovement
            {
                Id = Guid.NewGuid(),
                BusinessId = businessId,
                CatalogVariantId = variant.Id,
                Quantity = -quantity,
                Type = Domain.Enums.InventoryMovementType.Sale,
                CreatedAt = DateTime.UtcNow
            };

            _context.InventoryMovements.Add(movement);

            await _context.SaveChangesAsync();

            return $"Perfecto. Registré la compra de {quantity} {variant.CatalogItem.Name}. Total: ₡{totalAmount:N0}.";
        }
    }
}
