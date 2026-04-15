namespace MiNegocioCR.Api.Application.DTOs
{
    public class CatalogOptionValueDto
    {
        public Guid Id { get; set; }

        public Guid OptionId { get; set; }

        public string Value { get; set; } = string.Empty;

        public bool IsActive { get; set; }
    }
}
