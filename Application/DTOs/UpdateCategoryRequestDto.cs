namespace MiNegocioCR.Api.Application.DTOs
{
    public class UpdateCategoryRequestDto
    {
        public Guid BusinessId { get; set; }

        public string? Name { get; set; }
    }
}
