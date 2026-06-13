using MiNegocioCR.Api.Application.Common;
using MiNegocioCR.Api.Application.Interfaces.Repositories;
using MiNegocioCR.Api.Domain;
using MiNegocioCR.Api.Domain.Entities;
using MiNegocioCR.Api.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace MiNegocioCR.Api.Infrastructure.Persistence.Repositories
{
    public class VariantRepository : IVariantRepository
    {
        private readonly AppDbContext _context;

        public VariantRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<CatalogVariant?> GetVariantAsync(Guid variantId, Guid businessId)
        {
            return await _context.CatalogVariants
                .Where(v => v.Id == variantId)
                .Where(v => v.CatalogItem.BusinessId == businessId)
                .FirstOrDefaultAsync();
        }

        public async Task<List<CatalogVariant>> GetVariantsByItemAsync(Guid catalogItemId)
        {
            return await _context.CatalogVariants
                .Where(x => x.CatalogItemId == catalogItemId)
                .ToListAsync();
        }

        public async Task<List<CatalogVariant>> GetVariantsWithOptionDetailsByCatalogItemIdAsync(Guid catalogItemId)
        {
            return await _context.CatalogVariants
                .AsNoTracking()
                .AsSplitQuery()
                .Where(v => v.CatalogItemId == catalogItemId)
                .Include(v => v.VariantOptionValues)
                    .ThenInclude(l => l.CatalogOptionValue)
                        .ThenInclude(ov => ov.CatalogOption)
                .OrderBy(v => v.SKU ?? string.Empty)
                .ThenBy(v => v.Id)
                .ToListAsync();
        }

        public async Task<List<CatalogVariant>> GetVariantsWithOptionDetailsByBusinessAsync(
            Guid businessId,
            Guid? catalogItemId = null,
            string? search = null)
        {
            var query = _context.CatalogVariants
                .AsNoTracking()
                .AsSplitQuery()
                .Where(v => v.CatalogItem.BusinessId == businessId);

            if (catalogItemId.HasValue)
                query = query.Where(v => v.CatalogItemId == catalogItemId.Value);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim();
                query = query.Where(v =>
                    (v.SKU != null && EF.Functions.ILike(v.SKU, $"%{term}%")) ||
                    EF.Functions.ILike(v.CatalogItem.Name, $"%{term}%") ||
                    v.VariantOptionValues.Any(l =>
                        EF.Functions.ILike(l.CatalogOptionValue.Value, $"%{term}%") ||
                        EF.Functions.ILike(l.CatalogOptionValue.CatalogOption.Name, $"%{term}%")));
            }

            return await query
                .Include(v => v.CatalogItem)
                .Include(v => v.VariantOptionValues)
                    .ThenInclude(l => l.CatalogOptionValue)
                        .ThenInclude(ov => ov.CatalogOption)
                .OrderBy(v => v.CatalogItem.Name)
                .ThenBy(v => v.SKU ?? string.Empty)
                .ThenBy(v => v.Id)
                .ToListAsync();
        }

        public async Task<Dictionary<Guid, int>> GetInitialStockQuantitiesAsync(IReadOnlyCollection<Guid> variantIds)
        {
            if (variantIds == null || variantIds.Count == 0)
                return new Dictionary<Guid, int>();

            var rows = await _context.InventoryMovements
                .AsNoTracking()
                .Where(m => variantIds.Contains(m.CatalogVariantId))
                .Where(m =>
                    m.Type == InventoryMovementType.Purchase &&
                    m.Notes == InventoryMovementNotes.InitialStock)
                .Select(m => new { m.CatalogVariantId, m.Quantity })
                .ToListAsync();

            return rows
                .GroupBy(x => x.CatalogVariantId)
                .ToDictionary(g => g.Key, g => g.Sum(x => x.Quantity));
        }

        public async Task AddVariantAsync(CatalogVariant variant)
        {
            if (variant == null)
                throw new ArgumentNullException(nameof(variant));
            await _context.CatalogVariants.AddAsync(variant);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(CatalogVariant variant)
        {
            if (variant == null)
                throw new ArgumentNullException(nameof(variant));
            var entry = _context.Entry(variant);
            if (entry.State == EntityState.Detached)
                _context.CatalogVariants.Update(variant);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(CatalogVariant variant)
        {
            if (variant == null)
                throw new ArgumentNullException(nameof(variant));

            // InventoryMovements tienen FK con Restrict; hay que quitarlas antes de la variante.
            // DeleteVariantUseCase solo llega aquí si no hay historial “operativo” (p. ej. solo stock inicial).
            var movements = await _context.InventoryMovements
                .Where(m => m.CatalogVariantId == variant.Id)
                .ToListAsync();

            if (movements.Count > 0)
                _context.InventoryMovements.RemoveRange(movements);

            _context.CatalogVariants.Remove(variant);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// True si hay historial de inventario que debe impedir borrar la variante.
        /// No cuenta el movimiento de stock inicial al crear la variante (Purchase + nota estándar).
        /// </summary>
        public async Task<bool> ExistsInInventoryAsync(Guid variantId)
        {
            return await _context.InventoryMovements
                .AsNoTracking()
                .Where(m => m.CatalogVariantId == variantId)
                .AnyAsync(m =>
                    m.Type != InventoryMovementType.Purchase ||
                    m.Notes != InventoryMovementNotes.InitialStock);
        }

        public async Task<bool> ExistsInSalesAsync(Guid variantId)
        {
            return await _context.SaleItems
                .AsNoTracking()
                .AnyAsync(i => i.CatalogVariantId == variantId);
        }

        public async Task<bool> ExistsInPurchasesAsync(Guid variantId)
        {
            return await _context.PurchaseItems
                .AsNoTracking()
                .AnyAsync(i => i.CatalogVariantId == variantId);
        }

        public async Task<bool> ExistsInCreditsAsync(Guid variantId)
        {
            return await _context.CreditTransactionLines
                .AsNoTracking()
                .AnyAsync(l => l.CatalogVariantId == variantId);
        }

        public async Task<bool> ExistsInRepairOrdersAsync(Guid variantId)
        {
            return await _context.RepairOrderItems
                .AsNoTracking()
                .AnyAsync(i => i.CatalogVariantId == variantId);
        }

        public async Task<bool> ExistsSkuForCatalogItemAsync(
            Guid catalogItemId,
            string sku,
            Guid? excludeVariantId = null)
        {
            if (string.IsNullOrWhiteSpace(sku))
                return false;

            var normalized = sku.Trim().ToLowerInvariant();
            var query = _context.CatalogVariants
                .AsNoTracking()
                .Where(v => v.CatalogItemId == catalogItemId && v.SKU != null);

            if (excludeVariantId.HasValue)
                query = query.Where(v => v.Id != excludeVariantId.Value);

            return await query.AnyAsync(v => v.SKU!.ToLower() == normalized);
        }

        public async Task<bool> ExistsSkuForBusinessAsync(
            Guid businessId,
            string sku,
            Guid? excludeVariantId = null)
        {
            var key = SkuNormalizer.ToNormalizedKey(sku);
            if (key == null)
            {
                return false;
            }

            var query = _context.CatalogVariants
                .AsNoTracking()
                .Where(v => v.BusinessId == businessId && v.SkuNormalized == key);

            if (excludeVariantId.HasValue)
            {
                query = query.Where(v => v.Id != excludeVariantId.Value);
            }

            return await query.AnyAsync();
        }

        public async Task<CatalogVariant?> FindByBusinessAndSkuAsync(Guid businessId, string sku)
        {
            var key = SkuNormalizer.ToNormalizedKey(sku);
            if (key == null)
            {
                return null;
            }

            return await _context.CatalogVariants
                .AsNoTracking()
                .FirstOrDefaultAsync(v => v.BusinessId == businessId && v.SkuNormalized == key);
        }
    }
}
