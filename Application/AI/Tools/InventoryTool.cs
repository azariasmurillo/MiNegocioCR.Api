using Microsoft.EntityFrameworkCore;
using MiNegocioCR.Api.Application.AI.Interfaces;
using MiNegocioCR.Api.Application.AI.Models;
using MiNegocioCR.Api.Application.AI.Search;
using MiNegocioCR.Api.Domain.Enums;
using MiNegocioCR.Api.Infrastructure.Persistence;

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

        public async Task<ToolResult> ExecuteAsync(Guid businessId, string query)
        {
            query = query.ToLower().Trim();

            var normalizer = new SearchNormalizer();
            var terms = normalizer.Normalize(query);

            var variants = await _context.CatalogVariants
                .Include(v => v.CatalogItem)
                .Where(v =>
                    v.CatalogItem.BusinessId == businessId &&
                    v.IsActive)
                .ToListAsync();
            
            var items = variants
                .Where(v =>
                    terms.Any(term =>
                        v.CatalogItem.Name.ToLower().Contains(term)))
                .Select(v => new
                {
                    v.Id,
                    v.Price,
                    v.StockQuantity,
                    ItemName = v.CatalogItem!.Name,
                    ItemId = v.CatalogItemId
                })
                .ToList();


            if (!items.Any())
            {
                return new ToolResult
                {
                    Message = "No encontré productos relacionados en el inventario."
                };
            }

            var first = items.First();

            return new ToolResult
            {
                Message = $"Tenemos {first.ItemName} por ₡{first.Price}. Stock disponible: {first.StockQuantity}.",
                ProductId = first.Id,   // 👈 VARIANT ID
                ProductName = first.ItemName,
                Price = first.Price
            };
        }
    }
}
