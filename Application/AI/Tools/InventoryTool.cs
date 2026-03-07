using Microsoft.EntityFrameworkCore;
using MiNegocioCR.Api.Application.AI.Interfaces;
using MiNegocioCR.Api.Infrastructure.Persistence;
using System.Text.Json;

namespace MiNegocioCR.Api.Application.AI.Tools
{
    public class InventoryTool : IAITool
    {
        private readonly AppDbContext _context;

        public string Name => "inventory_search";

        public InventoryTool(AppDbContext context)
        {
            _context = context;
        }

        public async Task<string> ExecuteAsync(Guid businessId, string query)
        {
            query = query.ToLower();

            var items = await _context.CatalogVariants
            .Include(v => v.CatalogItem)
            .Where(v =>
                v.CatalogItem.BusinessId == businessId &&
                (v.CatalogItem.Name.ToLower().Contains(query) || query.Contains(v.CatalogItem.Name.ToLower())))
            .Select(v => new
            {
                v.Id,
                v.SKU,
                v.Price,
                v.StockQuantity,
                v.LowStockThreshold,
                v.IsActive,
                ItemName = v.CatalogItem!.Name,
                ItemId = v.CatalogItemId
            })
            .ToListAsync();
            return JsonSerializer.Serialize(items);
        }
    }
}
