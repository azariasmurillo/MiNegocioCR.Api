namespace MiNegocioCR.Api.Application.AI.Sales
{
    public interface ISaleService
    {
        Task<string> CreateSaleAsync(
            Guid businessId,
            Guid variantId,
            string phoneNumber,
            int quantity);
    }
}
