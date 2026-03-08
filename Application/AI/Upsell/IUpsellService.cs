using MiNegocioCR.Api.Domain.Entities;

namespace MiNegocioCR.Api.Application.AI.Upsell
{
    public interface IUpsellService
    {
        Task<List<CatalogItem>> GetUpsell(Guid businessId, Guid productId);
        Task<List<CatalogItem>> GetFallbackUpsell(Guid businessId);
    }
}
