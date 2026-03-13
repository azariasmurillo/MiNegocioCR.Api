namespace MiNegocioCR.Api.Application.Interfaces.Repositories
{
    public interface IRegisterPurchaseUseCase
    {
        Task ExecuteAsync(
            Guid businessId,
            Guid variantId,
            int quantity,
            decimal cost);
    }
}
