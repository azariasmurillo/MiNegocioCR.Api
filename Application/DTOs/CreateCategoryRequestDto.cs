namespace MiNegocioCR.Api.Application.DTOs
{
    public class CreateCategoryRequestDto
    {
        public Guid BusinessId { get; set; }

        public string? Name { get; set; }
    }
}
