namespace MiNegocioCR.Api.Application.DTOs
{
    public class ToggleCategoryStatusRequestDto
    {
        public Guid BusinessId { get; set; }

        public bool IsActive { get; set; }
    }
}
