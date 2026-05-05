using MiNegocioCR.Api.Application.DTOs;
using MiNegocioCR.Api.Application.Interfaces.RepairOrders;
using MiNegocioCR.Api.Application.Interfaces.UseCases.Sales;

namespace MiNegocioCR.Api.Application.UseCases.Sales;

public class CreateSaleFromRepairUseCase : ICreateSaleFromRepairUseCase
{
    private readonly IChargeRepairOrderUseCase _chargeRepairOrderUseCase;

    public CreateSaleFromRepairUseCase(IChargeRepairOrderUseCase chargeRepairOrderUseCase)
    {
        _chargeRepairOrderUseCase = chargeRepairOrderUseCase;
    }

    public async Task<object> ExecuteAsync(Guid businessId, Guid repairOrderId, CreateSaleFromRepairRequestDto request)
    {
        if (request == null) throw new ArgumentNullException(nameof(request));
        return await _chargeRepairOrderUseCase.Execute(businessId, repairOrderId);
    }
}
