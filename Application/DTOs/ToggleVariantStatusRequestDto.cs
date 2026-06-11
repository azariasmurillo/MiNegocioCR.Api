namespace MiNegocioCR.Api.Application.DTOs
{
    public class ToggleVariantStatusRequestDto
    {
        public Guid BusinessId { get; set; }

        public bool IsActive { get; set; }
    }
}
