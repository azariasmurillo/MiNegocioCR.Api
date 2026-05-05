using MiNegocioCR.Api.Application.DTOs;
using MiNegocioCR.Api.Application.Interfaces.UseCases.Sales;
using MiNegocioCR.Api.Application.Interfaces.RepairOrders;

namespace MiNegocioCR.Api.Application.UseCases.RepairOrder;

/// <summary>
/// Cobra la orden delegando en el mismo flujo que POST /api/sales (RegisterSale con RepairOrderId).
/// No calcula montos aquí: Payments, descuento % de la orden y totales viven en RegisterSaleUseCase.
/// </summary>
public class ChargeRepairOrderUseCase : IChargeRepairOrderUseCase
{
    private readonly IRegisterSaleUseCase _registerSaleUseCase;

    public ChargeRepairOrderUseCase(IRegisterSaleUseCase registerSaleUseCase)
    {
        _registerSaleUseCase = registerSaleUseCase;
    }

    public Task<object> Execute(Guid businessId, Guid repairOrderId)
    {
        if (businessId == Guid.Empty)
            throw new ArgumentException("BusinessId is required.", nameof(businessId));

        var request = new CreateSaleRequestDto
        {
            BusinessId = businessId,
            RepairOrderId = repairOrderId,
            Source = "Repair"
        };

        return _registerSaleUseCase.ExecuteAsync(request);
    }
}
