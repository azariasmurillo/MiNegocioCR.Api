namespace MiNegocioCR.Api.Application.Interfaces.Repositories
{
    public interface ICreateVariantUseCase
    {
        Task<Guid> ExecuteAsync(
            Guid catalogItemId,
            string sku,
            decimal price,
            int initialStock);
    }
}
