namespace MiNegocioCR.Api.Application.Interfaces.UseCases.Sales;

public interface IGetSaleByIdUseCase
{
    /// <returns>null si no existe o si <paramref name="businessId"/> no coincide con la venta.</returns>
    Task<object?> ExecuteAsync(Guid saleId, Guid? businessId = null);
}
