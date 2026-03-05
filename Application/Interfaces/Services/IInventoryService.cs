namespace MiNegocioCR.Api.Application.Interfaces.Services
{
    public interface IInventoryService
    {
        Task IncreaseStockAsync(Guid businessId, Guid variantId, int quantity, string reference);

        Task DecreaseStockAsync(Guid businessId, Guid variantId, int quantity, string reference);

        Task AdjustStockAsync(Guid businessId, Guid variantId, int quantity, string reason);
    }
}
