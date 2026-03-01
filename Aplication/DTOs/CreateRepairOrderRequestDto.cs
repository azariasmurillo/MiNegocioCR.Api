namespace MiNegocioCR.Api.Aplication.DTOs
{
    public class CreateRepairOrderRequestDto
    {
        public string CustomerName { get; set; } = string.Empty;
        public string? CustomerPhone { get; set; }
        public string? CustomerEmail { get; set; }
        public string? DeviceDescription { get; set; }
        public string? ProblemDescription { get; set; }
    }
}
