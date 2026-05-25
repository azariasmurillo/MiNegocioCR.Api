namespace MiNegocioCR.Api.Application.DTOs
{
    public class UpdateRepairOrderRequestDto
    {
        /// <summary>
        /// Si se envía, la orden pasa a este contacto (mismo negocio).
        /// </summary>
        public Guid? ContactId { get; set; }

        /// <summary>
        /// Datos del contacto si no se envía <see cref="ContactId"/>; también para crear/buscar por teléfono.
        /// </summary>
        public string? Name { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }

        public string? ProblemDescription { get; set; }
        public string? DeviceType { get; set; }
        public string? DeviceTypeOther { get; set; }
        public string? Brand { get; set; }
        public string? Model { get; set; }
        public string? SerialNumber { get; set; }
        public string? AccessoriesIncluded { get; set; }
        public string? OperatingSystem { get; set; }
        public string? Password { get; set; }
        public bool? IsDiagnosticPaid { get; set; }
        public decimal? DiscountPercent { get; set; }

        /// <summary>Null: no tocar ítems. No vacía: reemplaza todas las líneas. Lista vacía: quita los ítems.</summary>
        public List<RepairOrderItemDto>? Items { get; set; }
    }
}
