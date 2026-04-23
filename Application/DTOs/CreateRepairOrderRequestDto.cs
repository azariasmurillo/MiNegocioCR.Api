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
        public string? DeviceType { get; set; }
        public string? DeviceTypeOther { get; set; }
        public string? Brand { get; set; }
        public string? Model { get; set; }
        public string? SerialNumber { get; set; }
        public string? AccessoriesIncluded { get; set; }
        public string? OperatingSystem { get; set; }
        public string? Password { get; set; }

        public List<RepairOrderItemDto> Items { get; set; } = new();
    }
}
