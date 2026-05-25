namespace MiNegocioCR.Api.Application.DTOs
{
    public class CreateSaleRequestDto
    {
        public Guid BusinessId { get; set; }
        public Guid? RepairOrderId { get; set; }
        public Guid? ContactId { get; set; }
        public string? CustomerPhone { get; set; }
        public string? CustomerName { get; set; }
        public string? CustomerEmail { get; set; }

        /// <summary>Descuento en colones para ventas directas (no aplica cuando hay RepairOrderId).</summary>
        public decimal Discount { get; set; } = 0m;

        /// <summary>
        /// Métodos de pago con monto real. Reemplaza los booleans PayCash/etc.
        /// Suma de Amount debe ser >= Total de la venta (se valida en el use case).
        /// </summary>
        public List<SalePaymentMethodDto> PaymentMethods { get; set; } = new();

        public string Source { get; set; } = "Manual";

        public List<SaleItemRequestDto> Items { get; set; } = new();
    }
}
