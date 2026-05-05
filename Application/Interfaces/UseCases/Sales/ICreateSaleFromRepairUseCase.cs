using MiNegocioCR.Api.Application.DTOs;

namespace MiNegocioCR.Api.Application.Interfaces.UseCases.Sales
{
    public interface ICreateSaleFromRepairUseCase
    {
        Task<object> ExecuteAsync(Guid businessId, Guid repairOrderId, CreateSaleFromRepairRequestDto request);
    }
}
