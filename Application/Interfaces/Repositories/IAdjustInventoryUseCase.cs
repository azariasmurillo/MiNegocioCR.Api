namespace MiNegocioCR.Api.Application.Interfaces.Repositories
{
    public interface IAdjustInventoryUseCase
    {
        Task ExecuteAsync(
            Guid businessId,
            Guid variantId,
            int adjustment,
            string reason);
    }
}
