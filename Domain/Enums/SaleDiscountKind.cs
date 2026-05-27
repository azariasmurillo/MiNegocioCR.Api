namespace MiNegocioCR.Api.Domain.Enums;

/// <summary>Cómo se ingresó el descuento al facturar (metadata discreta).</summary>
public enum SaleDiscountKind : byte
{
    None = 0,
    Percent = 1,
    FixedAmount = 2
}
