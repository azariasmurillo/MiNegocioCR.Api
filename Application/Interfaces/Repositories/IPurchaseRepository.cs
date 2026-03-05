using MiNegocioCR.Api.Domain.Entities;

namespace MiNegocioCR.Api.Application.Interfaces.Repositories
{
    public interface IPurchaseRepository
    {
        Task AddPurchaseAsync(Purchase purchase);

        Task<List<Purchase>> GetPurchasesAsync(Guid businessId);

        Task<Purchase?> GetPurchaseAsync(Guid id, Guid businessId);
    }
}
