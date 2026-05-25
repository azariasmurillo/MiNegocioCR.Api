namespace MiNegocioCR.Api.Application.DTOs;

public class ChargeRepairOrderRequestDto
{
    /// <summary>
    /// Métodos de pago con montos reales para esta facturación final.
    /// Si se omite o está vacío, la venta se guarda sin desglose de métodos.
    /// </summary>
    public List<SalePaymentMethodDto> PaymentMethods { get; set; } = [];
}
