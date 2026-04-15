using MiNegocioCR.Api.Domain.Enums;

namespace MiNegocioCR.Api.Application.DTOs
{
    public class CreateCatalogItemRequestDto
    {
        public Guid BusinessId { get; set; }

        public string? Name { get; set; }

        public decimal BasePrice { get; set; }

        public bool TrackStock { get; set; }

        public CatalogItemType Type { get; set; }

        public Guid? CategoryId { get; set; }
    }
}
