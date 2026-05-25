namespace MiNegocioCR.Api.Application.DTOs;

/// <summary>
/// Representa un método de pago con monto real en una venta.
/// Soporta pagos mixtos: ej. ₡10,000 en efectivo + ₡25,000 por SINPE.
/// </summary>
public class SalePaymentMethodDto
{
    /// <summary>Método: Cash | Transfer | Sinpe | Card</summary>
    public string Method { get; set; } = string.Empty;

    /// <summary>Monto en colones asignado a este método.</summary>
    public decimal Amount { get; set; }
}
