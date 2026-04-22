namespace MiNegocioCR.Api.Application.Interfaces
{
    namespace MiNegocioCR.Api.Application.Interfaces.UseCases.Sales
    {
        public interface IRegisterSaleUseCase
        {
            Task<Guid> ExecuteAsync(
                Guid businessId,
                List<(Guid variantId, int quantity, decimal price)> items,
                string? customerPhone = null,
                string? customerName = null,
                string? customerEmail = null);
        }
    }
}
