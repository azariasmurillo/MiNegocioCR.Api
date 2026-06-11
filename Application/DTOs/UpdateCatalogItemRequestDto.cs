namespace MiNegocioCR.Api.Application.DTOs
{
    public class UpdateCatalogItemRequestDto
    {
        public Guid BusinessId { get; set; }

        public string? Name { get; set; }

        public decimal BasePrice { get; set; }

        public Guid? CategoryId { get; set; }

        public bool TrackStock { get; set; }

        public Domain.Enums.CatalogItemType Type { get; set; }

        public string? Description { get; set; }
    }
}
