namespace MiNegocioCR.Api.Application.DTOs
{
    public class CreateRepairOrderRequestDto
    {
        /// <summary>
        /// Si viene informado, se usa este contacto (debe pertenecer al negocio). Si no, se resuelve por teléfono.
        /// </summary>
        public Guid? ContactId { get; set; }

        public string CustomerName { get; set; } = string.Empty;
        public string CustomerPhone { get; set; } = string.Empty;
        public string? CustomerEmail { get; set; }
        public string? DeviceDescription { get; set; }
        public string? ProblemDescription { get; set; }
    }
}
