using MiNegocioCR.Api.Domain.Enums;

namespace MiNegocioCR.Api.Domain.Entities;

/// <summary>
/// Registra un método y monto de pago aplicado a una venta.
/// Reemplaza los booleans PayCash/PayTransfer/PaySinpe/PayCard para soportar
/// pagos mixtos con montos reales (ej. ₡10,000 efectivo + ₡25,000 SINPE).
/// </summary>
public class SalePaymentMethod
{
    public Guid Id { get; set; }
    public Guid SaleId { get; set; }
    public PaymentMethod Method { get; set; }
    public decimal Amount { get; set; }

    public Sale Sale { get; set; } = null!;
}
