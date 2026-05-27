using MiNegocioCR.Api.Application.DTOs;
using MiNegocioCR.Api.Application.Interfaces.UseCases.Sales;

namespace MiNegocioCR.Api.Application.UseCases.Sales;

/// <summary>
/// Factura el saldo pendiente de una orden de reparación.
/// POST /api/sales/from-repair/{repairOrderId}
///
/// El backend siempre es authoritative: líneas, descuento e impuestos se leen
/// desde la orden en BD. El request solo aporta los métodos de pago con montos.
/// </summary>
public class CreateSaleFromRepairUseCase : ICreateSaleFromRepairUseCase
{
    private readonly IRegisterSaleUseCase _registerSaleUseCase;

    public CreateSaleFromRepairUseCase(IRegisterSaleUseCase registerSaleUseCase)
    {
        _registerSaleUseCase = registerSaleUseCase;
    }

    public async Task<object> ExecuteAsync(Guid businessId, Guid repairOrderId, CreateSaleFromRepairRequestDto request)
    {
        if (request == null) throw new ArgumentNullException(nameof(request));

        var saleRequest = new CreateSaleRequestDto
        {
            BusinessId = businessId,
            RepairOrderId = repairOrderId,
            Source = "FromRepair",
            PaymentMethods = request.PaymentMethods ?? new(),
            DiscountKind = request.DiscountKind,
            DiscountValue = request.DiscountValue,
            Discount = request.Discount,
        };

        return await _registerSaleUseCase.ExecuteAsync(saleRequest);
    }
}
