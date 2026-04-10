namespace MiNegocioCR.Api.Application.Interfaces.Repositories
{
    public interface IRegisterPurchaseUseCase
    {
        Task ExecuteAsync(
            Guid businessId,
            IReadOnlyList<(Guid VariantId, int Quantity, decimal Cost)> items);
    }
}
