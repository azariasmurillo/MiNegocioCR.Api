namespace MiNegocioCR.Api.Application.DTOs;

public class CreateSaleFromRepairRequestDto
{
    public Guid BusinessId { get; set; }

    /// <summary>
    /// Métodos de pago con monto real elegidos al facturar el saldo pendiente.
    /// Suma de Amount debe ser >= SaldoPendiente de la orden.
    /// </summary>
    public List<SalePaymentMethodDto> PaymentMethods { get; set; } = new();
}
