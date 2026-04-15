namespace MiNegocioCR.Api.Application.DTOs
{
    public class ToggleCatalogItemStatusRequestDto
    {
        public Guid BusinessId { get; set; }

        public bool IsActive { get; set; }
    }
}
