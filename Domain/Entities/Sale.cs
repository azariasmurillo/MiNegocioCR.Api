namespace MiNegocioCR.Api.Domain.Entities
{
    public class Sale
    {
        public Guid Id { get; set; }

        public Guid BusinessId { get; set; }
        public string InvoiceNumber { get; set; } = string.Empty;
        public string? HaciendaConsecutive { get; set; }
        public Guid? RepairOrderId { get; set; }
        public string Source { get; set; } = "Manual";

        // ── Totales ────────────────────────────────────────────────────────
        public decimal Subtotal { get; set; }

        /// <summary>Descuento REAL aplicado (colones), nunca incluye abonos/prepagos.</summary>
        public decimal DiscountAmount { get; set; }

        /// <summary>0=None, 1=Percent, 2=FixedAmount — metadata de cómo se ingresó el descuento.</summary>
        public byte DiscountKind { get; set; }

        /// <summary>Valor ingresado (10 para 10%, o 5000 para ₡5000).</summary>
        public decimal DiscountInputValue { get; set; }

        /// <summary>Impuesto (IVA) calculado sobre (Subtotal − DiscountAmount).</summary>
        public decimal TaxAmount { get; set; }

        /// <summary>
        /// Total bruto de la orden: Subtotal − DiscountAmount + TaxAmount.
        /// Para ventas directas coincide con <see cref="Total"/>; para ventas desde
        /// reparación puede ser mayor cuando había abonos previos.
        /// </summary>
        public decimal TotalOrden { get; set; }

        /// <summary>
        /// Suma de abonos (Payments) registrados en la orden de reparación ANTES
        /// de esta factura. Siempre 0 para ventas directas.
        /// </summary>
        public decimal PrepaidAmount { get; set; }

        /// <summary>Monto cobrado hoy = TotalOrden − PrepaidAmount (saldo pendiente facturado).</summary>
        public decimal Total { get; set; }

        // ── Métricas de rentabilidad ───────────────────────────────────────
        /// <summary>Suma de costos de línea (CostPrice × cantidad) al momento de la venta.</summary>
        public decimal TotalCost { get; set; }

        /// <summary>Ganancia: Total − TotalCost.</summary>
        public decimal TotalProfit { get; set; }

        // ── Contacto / cliente ─────────────────────────────────────────────
        public Guid? ContactId { get; set; }
        public Contact? Contact { get; set; }
        public string CustomerPhone { get; set; } = string.Empty;

        // ── Fechas ─────────────────────────────────────────────────────────
        public DateTime SaleDate { get; set; }
        public DateTime CreatedAt { get; set; }

        // ── Navegación ─────────────────────────────────────────────────────
        public ICollection<SaleItem> Items { get; set; } = new List<SaleItem>();
        public ICollection<SalePaymentMethod> PaymentMethods { get; set; } = new List<SalePaymentMethod>();

        // ── Campo legacy (solo para queries existentes; no usar en código nuevo) ──
        /// <summary>Alias de <see cref="Total"/>; mantiene compatibilidad con código existente.</summary>
        public decimal TotalAmount { get; set; }
    }
}
