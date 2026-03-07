using Microsoft.EntityFrameworkCore;
using MiNegocioCR.Api.Domain.Entities;
using MiNegocioCR.Api.Infrastructure.Persistence;

namespace MiNegocioCR.Api.Application.AI.Sales
{
    public class SaleService
    {
        private readonly AppDbContext _context;

        public SaleService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<string> CreateSaleAsync(
            Guid businessId,
            Guid variantId,
            string phoneNumber)
        {
            var variant = await _context.CatalogVariants
                .Include(v => v.CatalogItem)
                .FirstOrDefaultAsync(v => v.Id == variantId);

            if (variant == null)
                return "Producto no encontrado.";

            if (variant.StockQuantity <= 0)
                return "Lo siento, ese producto está agotado.";

            var sale = new Sale
            {
                Id = Guid.NewGuid(),
                BusinessId = businessId,
                CreatedAt = DateTime.UtcNow,
                CustomerPhone = phoneNumber,
                TotalAmount = variant.Price
            };

            _context.Sales.Add(sale);

            var item = new SaleItem
            {
                Id = Guid.NewGuid(),
                SaleId = sale.Id,
                CatalogVariantId = variant.Id,
                Quantity = 1,
                UnitPrice = variant.Price,
                Total = variant.Price
            };

            _context.SaleItems.Add(item);

            variant.StockQuantity -= 1;

            var movement = new InventoryMovement
            {
                Id = Guid.NewGuid(),
                BusinessId = businessId,
                CatalogVariantId = variant.Id,
                Quantity = -1,
                Type = Domain.Enums.InventoryMovementType.Sale,
                CreatedAt = DateTime.UtcNow
            };

            _context.InventoryMovements.Add(movement);

            await _context.SaveChangesAsync();

            return $"Compra confirmada ✅\nProducto: {variant.CatalogItem.Name}\nPrecio: ₡{variant.Price}";
        }
    }
}
