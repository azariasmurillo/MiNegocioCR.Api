namespace MiNegocioCR.Api.Application.DTOs
{
    public class CreateSaleRequestDto
    {
        public List<SaleItemRequestDto> Items { get; set; } = new();
    }
}
