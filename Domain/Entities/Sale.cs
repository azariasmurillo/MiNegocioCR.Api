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
        public decimal Subtotal { get; set; }

        /// <summary>Monto del descuento aplicado (colones), no porcentaje.</summary>
        public decimal DiscountAmount { get; set; }

        /// <summary>Monto del impuesto (IVA u otro) aplicado (colones).</summary>
        public decimal TaxAmount { get; set; }
        public bool PayCash { get; set; } = false;
        public bool PayTransfer { get; set; } = false;
        public bool PaySinpe { get; set; } = false;
        public bool PayCard { get; set; } = false;

        public Guid? ContactId { get; set; }
        public Contact? Contact { get; set; }

        public DateTime SaleDate { get; set; }

        public decimal Total { get; set; }

        /// <summary>Suma de costos de línea (CostPrice × cantidad) al momento de la venta.</summary>
        public decimal TotalCost { get; set; }

        /// <summary>Ganancia respecto al total cobrado: <see cref="Total"/> − <see cref="TotalCost"/>.</summary>
        public decimal TotalProfit { get; set; }

        public ICollection<SaleItem> Items { get; set; } = new List<SaleItem>();
        public DateTime CreatedAt { get; internal set; }
        public string CustomerPhone { get; internal set; } = string.Empty;
        public decimal TotalAmount { get; internal set; }
    }
}
