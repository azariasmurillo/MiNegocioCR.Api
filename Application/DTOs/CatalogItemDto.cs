using MiNegocioCR.Api.Domain.Enums;

namespace MiNegocioCR.Api.Application.DTOs
{
    public class CatalogItemDto
    {
        public Guid Id { get; set; }

        public Guid BusinessId { get; set; }

        public Guid? CategoryId { get; set; }

        public string Name { get; set; } = string.Empty;

        public decimal BasePrice { get; set; }

        public bool TrackStock { get; set; }

        public CatalogItemType Type { get; set; }

        public bool IsActive { get; set; }
    }
}
