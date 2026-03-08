using Microsoft.EntityFrameworkCore;
using MiNegocioCR.Api.Application.Interfaces;
using MiNegocioCR.Api.Domain.Entities;

namespace MiNegocioCR.Api.Application.AI.Upsell
{
    public class UpsellService : IUpsellService
    {
        private readonly IAppDbContext _context;

        public UpsellService(IAppDbContext context)
        {
            _context = context;
        }

        public async Task<List<CatalogItem>> GetUpsell(Guid businessId, Guid productId)
        {
            var rules = await _context.UpsellRules
                .Where(x => x.BusinessId == businessId && x.ProductId == productId)
                .ToListAsync();

            var suggestedIds = rules.Select(x => x.SuggestedProductId);

            return await _context.CatalogItems
                .Where(x => suggestedIds.Contains(x.Id))
                .ToListAsync();
        }

        public async Task<List<CatalogItem>> GetFallbackUpsell(Guid businessId)
        {
            return await _context.CatalogItems
                .Where(x => x.BusinessId == businessId && x.IsActive)
                .OrderByDescending(x => x.CreatedAt)
                .Take(3)
                .ToListAsync();
        }
    }
}
