namespace MiNegocioCR.Api.Application.DTOs
{
    public class CreateCatalogOptionValueRequestDto
    {
        public Guid OptionId { get; set; }

        public string? Value { get; set; }
    }
}
