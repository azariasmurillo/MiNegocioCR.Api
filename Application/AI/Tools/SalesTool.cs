using Microsoft.EntityFrameworkCore;
using MiNegocioCR.Api.Application.AI.Interfaces;
using MiNegocioCR.Api.Infrastructure.Persistence;

namespace MiNegocioCR.Api.Application.AI.Tools
{
    public class SalesTool : IAITool
    {
        private readonly AppDbContext _context;

        public string Name => "sales_prepare";

        public SalesTool(AppDbContext context)
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
                    v.CatalogItem.Name.ToLower().Contains(query))
                .Select(v => new
                {
                    v.Id,
                    v.Price,
                    v.StockQuantity,
                    ItemName = v.CatalogItem.Name
                })
                .ToListAsync();

            if (!items.Any())
                return "No encontramos productos con ese nombre.";

            return System.Text.Json.JsonSerializer.Serialize(items);
        }
    }
}
