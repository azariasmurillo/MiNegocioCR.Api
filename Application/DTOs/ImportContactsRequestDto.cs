namespace MiNegocioCR.Api.Application.DTOs
{
    public class ImportContactsRequestDto
    {
        public Guid BusinessId { get; set; }
        public List<ContactDto> Contacts { get; set; } = new();
    }
}
