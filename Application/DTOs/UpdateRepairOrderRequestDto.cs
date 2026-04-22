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

        public string? DeviceDescription { get; set; }
        public string? ProblemDescription { get; set; }
    }
}
