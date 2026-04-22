namespace MiNegocioCR.Api.Application.DTOs
{
    public class CreateSaleRequestDto
    {
        public Guid BusinessId { get; set; }

        public string? CustomerPhone { get; set; }
        public string? CustomerName { get; set; }
        public string? CustomerEmail { get; set; }

        public List<SaleItemRequestDto> Items { get; set; } = new();
    }
}
