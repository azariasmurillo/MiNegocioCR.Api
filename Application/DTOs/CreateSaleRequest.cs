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
        public decimal Discount { get; set; } = 0m;
        public bool PayCash { get; set; }
        public bool PayTransfer { get; set; }
        public bool PaySinpe { get; set; }
        public bool PayCard { get; set; }
        public string Source { get; set; } = "Manual"; // Manual | WhatsApp

        public List<SaleItemRequestDto> Items { get; set; } = new();
    }
}
