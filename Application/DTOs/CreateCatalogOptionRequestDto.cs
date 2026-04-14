namespace MiNegocioCR.Api.Application.DTOs
{
    public class CreateCatalogOptionRequestDto
    {
        public Guid CatalogItemId { get; set; }

        public string? Name { get; set; }
    }
}
