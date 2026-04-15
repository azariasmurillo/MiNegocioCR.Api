namespace MiNegocioCR.Api.Application.DTOs
{
    public class CatalogOptionDto
    {
        public Guid Id { get; set; }

        public Guid CatalogItemId { get; set; }

        public string Name { get; set; } = string.Empty;

        public bool IsActive { get; set; }
    }
}
