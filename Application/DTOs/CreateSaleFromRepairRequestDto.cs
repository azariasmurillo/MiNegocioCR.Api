namespace MiNegocioCR.Api.Application.DTOs;

public class CreateSaleFromRepairRequestDto
{
    public Guid BusinessId { get; set; }

    /// <summary>
    /// Métodos de pago con monto real elegidos al facturar el saldo pendiente.
    /// </summary>
    public List<SalePaymentMethodDto> PaymentMethods { get; set; } = new();

    /// <summary>None | Percent | FixedAmount</summary>
    public string DiscountKind { get; set; } = "None";

    /// <summary>Valor ingresado (% o colones según DiscountKind).</summary>
    public decimal DiscountValue { get; set; } = 0m;

    /// <summary>Monto en colones (legacy / fallback).</summary>
    public decimal Discount { get; set; } = 0m;
}
