namespace MiNegocioCR.Api.Application.Interfaces.Repositories
{
    public interface IDeleteVariantUseCase
    {
        Task ExecuteAsync(Guid variantId, Guid businessId);
    }
}
